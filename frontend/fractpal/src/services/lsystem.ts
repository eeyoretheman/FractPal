// L-System Types
export interface Cursor {
  x_position: number;
  y_position: number;
  direction: number;
}

export interface Line {
  x1: number;
  y1: number;
  x2: number;
  y2: number;
  thickness: number;
  color: [number, number, number, number];
}

export type Drawable = Line;

// Generate L-System
export function lindenmayer(
  symbols: string[],
  substitutions: Map<string, string[]>,
  generations: number
): string[] {
  let currentGeneration: string[] = [...symbols];

  for (let i = 1; i <= generations; i++) {
    const newGeneration: string[] = [];

    currentGeneration.forEach((symbol) => {
      const replacement = substitutions.get(symbol);
      if (replacement) {
        newGeneration.push(...replacement);
      } else {
        newGeneration.push(symbol);
      }
    });

    currentGeneration = newGeneration;
  }

  return currentGeneration;
}

// Process symbol and return drawable
export function processSymbol(
  symbol: string,
  stack: string[],
  cursor: Cursor
): Drawable | undefined {
  switch (symbol) {
    case 'FORWARD': {
      const x1 = cursor.x_position;
      const y1 = cursor.y_position;

      const steps: number = Number(stack.pop());
      cursor.x_position += Math.cos(cursor.direction) * steps;
      cursor.y_position += Math.sin(cursor.direction) * steps;

      const x2 = cursor.x_position;
      const y2 = cursor.y_position;

      return {
        x1,
        y1,
        x2,
        y2,
        thickness: 1,
        color: [0, 0, 0, 1],
      };
    }
    case 'ROTATE': {
      const angle: number = Number(stack.pop());
      cursor.direction += (angle * Math.PI) / 180;
      return undefined;
    }
    default:
      stack.push(symbol);
      return undefined;
  }
}

// Draw line on WebGL context
export function drawLine(gl: WebGL2RenderingContext, line: Line) {
  const dx = line.x2 - line.x1;
  const dy = line.y2 - line.y1;
  const length = Math.hypot(dx, dy);

  if (length === 0) {
    return;
  }

  // Perpendicular Vector
  const nx = -dy / length;
  const ny = dx / length;
  const h = line.thickness / 2;

  const vertices = new Float32Array([
    line.x1 + nx * h,
    line.y1 + ny * h,
    line.x1 - nx * h,
    line.y1 - ny * h,
    line.x2 + nx * h,
    line.y2 + ny * h,

    line.x2 + nx * h,
    line.y2 + ny * h,
    line.x1 - nx * h,
    line.y1 - ny * h,
    line.x2 - nx * h,
    line.y2 - ny * h,
  ]);

  const buffer = gl.createBuffer();
  if (!buffer) return;
  gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
  gl.bufferData(gl.ARRAY_BUFFER, vertices, gl.STATIC_DRAW);

  gl.vertexAttribPointer(0, 2, gl.FLOAT, false, 0, 0);
  gl.enableVertexAttribArray(0);

  const program = gl.getParameter(gl.CURRENT_PROGRAM) as WebGLProgram;
  const colorLocation = gl.getUniformLocation(program, 'u_color');
  if (colorLocation) gl.uniform4fv(colorLocation, line.color);

  gl.drawArrays(gl.TRIANGLES, 0, 6);

  gl.deleteBuffer(buffer);
}

// Main turtle graphics function
export function turtle(symbols: string[], gl: WebGL2RenderingContext) {
  const stack: string[] = [];

  const cursor: Cursor = {
    x_position: 0,
    y_position: 0,
    direction: 0,
  };

  for (const symbol of symbols) {
    const line = processSymbol(symbol, stack, cursor);

    if (line) {
      drawLine(gl, line);
    }
  }
}

// Setup WebGL program
export function setupWebGL(canvas: HTMLCanvasElement, xTranslation: number, yTranslation: number, zoom: number): WebGL2RenderingContext | null {
  const gl = canvas.getContext('webgl2');
  if (!gl) {
    alert('WebGL2 not supported');
    return null;
  }

  gl.viewport(0, 0, canvas.width, canvas.height);
  gl.clearColor(1, 1, 1, 1);
  gl.clear(gl.COLOR_BUFFER_BIT);

  const vertexSource = `#version 300 es
    in vec2 a_position;
    uniform vec2 u_resolution;
    uniform vec2 u_pan;
    uniform float u_zoom;

    void main() {
      vec2 pos = (a_position + u_pan) * u_zoom;
      vec2 zeroToOne = pos / u_resolution;
      vec2 zeroToTwo = zeroToOne * 2.0;
      vec2 clipSpace = zeroToTwo - 1.0;
      gl_Position = vec4(clipSpace * vec2(1, -1), 0, 1);
    }
  `;

  const fragmentSource = `#version 300 es
    precision mediump float;
    uniform vec4 u_color;
    out vec4 outColor;

    void main() {
      outColor = u_color;
    }
  `;

  const program = gl.createProgram()!;

  const vertexShader = gl.createShader(gl.VERTEX_SHADER)!;
  gl.shaderSource(vertexShader, vertexSource);
  gl.compileShader(vertexShader);

  if (!gl.getShaderParameter(vertexShader, gl.COMPILE_STATUS)) {
    throw new Error(
      gl.getShaderInfoLog(vertexShader) || 'Vertex shader compilation failed.'
    );
  }

  const fragmentShader = gl.createShader(gl.FRAGMENT_SHADER)!;
  gl.shaderSource(fragmentShader, fragmentSource);
  gl.compileShader(fragmentShader);

  if (!gl.getShaderParameter(fragmentShader, gl.COMPILE_STATUS)) {
    throw new Error(
      gl.getShaderInfoLog(fragmentShader) || 'Fragment shader compilation failed.'
    );
  }

  gl.attachShader(program, vertexShader);
  gl.attachShader(program, fragmentShader);
  gl.linkProgram(program);

  if (!gl.getProgramParameter(program, gl.LINK_STATUS)) {
    throw new Error(
      gl.getProgramInfoLog(program) || 'Program linking failed.'
    );
  }

  gl.useProgram(program);

  const resolutionLocation = gl.getUniformLocation(program, 'u_resolution');
  gl.uniform2f(resolutionLocation, canvas.width, canvas.height);

  const panLocation = gl.getUniformLocation(program, 'u_pan');
  gl.uniform2f(panLocation, xTranslation, yTranslation);

  const zoomLocation = gl.getUniformLocation(program, 'u_zoom');
  gl.uniform1f(zoomLocation, zoom);

  return gl;
}

// Resize canvas to match display size
export function resizeCanvas(canvas: HTMLCanvasElement) {
  const dpr = window.devicePixelRatio || 1;
  const displayWidth = canvas.clientWidth;
  const displayHeight = canvas.clientHeight;

  if (canvas.width !== displayWidth * dpr || canvas.height !== displayHeight * dpr) {
    canvas.width = displayWidth * dpr;
    canvas.height = displayHeight * dpr;
  }
}
