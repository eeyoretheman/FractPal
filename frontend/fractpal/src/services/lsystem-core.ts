export interface Cursor {
  x_position: number;
  y_position: number;
  direction: number;
}

export interface FractalRenderDefinition {
  axiom: string;
  rules: string;
  instructions: string;
  generations: number;
}

export interface WorkerRequest {
  requestId: number;
  definition: FractalRenderDefinition;
}

export interface WorkerResponse {
  requestId: number;
  vertices?: Float32Array;
  error?: string;
}

function tokenize(input: string): string[] {
  const normalized = input.trim().replace(/\s+/g, ' ');
  return normalized ? normalized.split(' ') : [];
}

export function parseAxiom(axiom: string): string[] {
  return tokenize(axiom);
}

export function parseSubstitutions(source: string): Map<string, string[]> {
  const substitutions = new Map<string, string[]>();

  for (const line of source.split('\n')) {
    const trimmedLine = line.trim();
    if (!trimmedLine) {
      continue;
    }

    const [rawSymbol, rawReplacement, ...rest] = trimmedLine.split('=');
    if (!rawSymbol || !rawReplacement || rest.length > 0) {
      continue;
    }

    substitutions.set(rawSymbol.trim(), tokenize(rawReplacement));
  }

  return substitutions;
}

export function lindenmayer(
  symbols: string[],
  substitutions: Map<string, string[]>,
  generations: number
): string[] {
  let currentGeneration = [...symbols];

  for (let i = 0; i < generations; i += 1) {
    const nextGeneration: string[] = [];

    for (const symbol of currentGeneration) {
      const replacement = substitutions.get(symbol);
      if (replacement) {
        nextGeneration.push(...replacement);
      } else {
        nextGeneration.push(symbol);
      }
    }

    currentGeneration = nextGeneration;
  }

  return currentGeneration;
}

export function applyInstructions(
  symbols: string[],
  instructions: Map<string, string[]>
): string[] {
  const processedSymbols: string[] = [];

  for (const symbol of symbols) {
    const replacement = instructions.get(symbol);
    if (replacement) {
      processedSymbols.push(...replacement);
    } else {
      processedSymbols.push(symbol);
    }
  }

  return processedSymbols;
}

function appendLineVertices(
  out: number[],
  x1: number,
  y1: number,
  x2: number,
  y2: number,
  thickness: number
) {
  const dx = x2 - x1;
  const dy = y2 - y1;
  const length = Math.hypot(dx, dy);

  if (length === 0) {
    return;
  }

  const nx = -dy / length;
  const ny = dx / length;
  const halfThickness = thickness / 2;

  out.push(
    x1 + nx * halfThickness, y1 + ny * halfThickness,
    x1 - nx * halfThickness, y1 - ny * halfThickness,
    x2 + nx * halfThickness, y2 + ny * halfThickness,

    x2 + nx * halfThickness, y2 + ny * halfThickness,
    x1 - nx * halfThickness, y1 - ny * halfThickness,
    x2 - nx * halfThickness, y2 - ny * halfThickness
  );
}

export function turtleToVertices(symbols: string[]): Float32Array {
  const stack: string[] = [];
  const cursor: Cursor = {
    x_position: 0,
    y_position: 0,
    direction: 0,
  };
  const vertices: number[] = [];

  for (const symbol of symbols) {
    switch (symbol) {
      case 'FORWARD': {
        const steps = Number(stack.pop());
        if (!Number.isFinite(steps)) {
          continue;
        }

        const x1 = cursor.x_position;
        const y1 = cursor.y_position;

        cursor.x_position += Math.cos(cursor.direction) * steps;
        cursor.y_position += Math.sin(cursor.direction) * steps;

        appendLineVertices(
          vertices,
          x1,
          y1,
          cursor.x_position,
          cursor.y_position,
          1
        );
        break;
      }

      case 'ROTATE': {
        const angle = Number(stack.pop());
        if (!Number.isFinite(angle)) {
          continue;
        }

        cursor.direction += (angle * Math.PI) / 180;
        break;
      }

      default:
        stack.push(symbol);
    }
  }

  return new Float32Array(vertices);
}

export function generateVertices(definition: FractalRenderDefinition): Float32Array {
  const axiom = parseAxiom(definition.axiom);
  const rules = parseSubstitutions(definition.rules);
  const instructions = parseSubstitutions(definition.instructions);
  const expandedSymbols = lindenmayer(axiom, rules, definition.generations);
  const processedSymbols = applyInstructions(expandedSymbols, instructions);

  return turtleToVertices(processedSymbols);
}
