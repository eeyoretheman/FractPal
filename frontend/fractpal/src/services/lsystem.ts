import {
  generateVertices,
  type FractalRenderDefinition,
  type WorkerRequest,
  type WorkerResponse,
} from './lsystem-core';

export type { FractalRenderDefinition } from './lsystem-core';

export function createLSystemWorker(): Worker | null {
  if (typeof Worker === 'undefined') {
    return null;
  }

  return new Worker(new URL('./lsystem-worker.ts', import.meta.url), {
    type: 'module',
  });
}

export function requestVertices(
  worker: Worker | null,
  definition: FractalRenderDefinition,
  requestId: number
): Promise<Float32Array> {
  if (!worker) {
    return Promise.resolve(generateVertices(definition));
  }

  return new Promise<Float32Array>((resolve, reject) => {
    const handleMessage = (event: MessageEvent<WorkerResponse>) => {
      if (event.data.requestId !== requestId) {
        return;
      }

      cleanup();

      if (event.data.error) {
        reject(new Error(event.data.error));
        return;
      }

      resolve(event.data.vertices ?? new Float32Array());
    };

    const handleError = (event: ErrorEvent) => {
      cleanup();
      reject(event.error instanceof Error ? event.error : new Error(event.message));
    };

    const cleanup = () => {
      worker.removeEventListener('message', handleMessage);
      worker.removeEventListener('error', handleError);
    };

    worker.addEventListener('message', handleMessage);
    worker.addEventListener('error', handleError);

    const payload: WorkerRequest = { requestId, definition };
    worker.postMessage(payload);
  });
}

export function setupWebGL(
  canvas: HTMLCanvasElement,
  xTranslation: number,
  yTranslation: number,
  zoom: number
): WebGL2RenderingContext | null {
  const gl = canvas.getContext('webgl2', { preserveDrawingBuffer: true });
  if (!gl) {
    alert('WebGL2 not supported');
    return null;
  }

  gl.viewport(0, 0, canvas.width, canvas.height);
  gl.clearColor(1, 1, 1, 1);
  gl.clear(gl.COLOR_BUFFER_BIT);

  const vertexSource = `#version 300 es
    layout(location = 0) in vec2 a_position;
    uniform vec2 u_resolution;
    uniform vec2 u_pan;
    uniform float u_zoom;

    void main() {
      vec2 pos = (a_position + u_pan) * u_zoom;
      vec2 zeroToOne = pos / u_resolution;
      vec2 zeroToTwo = zeroToOne * 2.0;
      vec2 clipSpace = zeroToTwo - 1.0;
      gl_Position = vec4(clipSpace * vec2(1.0, -1.0), 0.0, 1.0);
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
    throw new Error(gl.getShaderInfoLog(vertexShader) || 'Vertex shader compilation failed.');
  }

  const fragmentShader = gl.createShader(gl.FRAGMENT_SHADER)!;
  gl.shaderSource(fragmentShader, fragmentSource);
  gl.compileShader(fragmentShader);
  if (!gl.getShaderParameter(fragmentShader, gl.COMPILE_STATUS)) {
    throw new Error(gl.getShaderInfoLog(fragmentShader) || 'Fragment shader compilation failed.');
  }

  gl.attachShader(program, vertexShader);
  gl.attachShader(program, fragmentShader);
  gl.linkProgram(program);
  if (!gl.getProgramParameter(program, gl.LINK_STATUS)) {
    throw new Error(gl.getProgramInfoLog(program) || 'Program linking failed.');
  }

  gl.useProgram(program);

  const resolutionLocation = gl.getUniformLocation(program, 'u_resolution');
  gl.uniform2f(resolutionLocation, canvas.width, canvas.height);

  const panLocation = gl.getUniformLocation(program, 'u_pan');
  gl.uniform2f(panLocation, xTranslation, yTranslation);

  const zoomLocation = gl.getUniformLocation(program, 'u_zoom');
  gl.uniform1f(zoomLocation, zoom);

  const colorLocation = gl.getUniformLocation(program, 'u_color');
  gl.uniform4fv(colorLocation, [0, 0, 0, 1]);

  return gl;
}

export function drawVertices(gl: WebGL2RenderingContext, vertices: Float32Array) {
  if (vertices.length === 0) {
    return;
  }

  const buffer = gl.createBuffer();
  if (!buffer) return;

  gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
  gl.bufferData(gl.ARRAY_BUFFER, vertices, gl.STATIC_DRAW);

  gl.enableVertexAttribArray(0);
  gl.vertexAttribPointer(0, 2, gl.FLOAT, false, 0, 0);

  gl.drawArrays(gl.TRIANGLES, 0, vertices.length / 2);

  gl.deleteBuffer(buffer);
}

export function resizeCanvas(canvas: HTMLCanvasElement) {
  const dpr = window.devicePixelRatio || 1;
  const displayWidth = canvas.clientWidth;
  const displayHeight = canvas.clientHeight;

  if (
    canvas.width !== displayWidth * dpr ||
    canvas.height !== displayHeight * dpr
  ) {
    canvas.width = displayWidth * dpr;
    canvas.height = displayHeight * dpr;
  }
}
