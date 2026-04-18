/// <reference lib="webworker" />
export { };

declare const self: DedicatedWorkerGlobalScope;

export interface Cursor {
    x_position: number;
    y_position: number;
    direction: number;
}

export interface WorkerRequest {
    symbols: string[];
    substitutions: Array<[string, string[]]>;
    generations: number;
}

export interface WorkerResponse {
    vertices: Float32Array;
}

function lindenmayer(
    symbols: string[],
    substitutions: Map<string, string[]>,
    generations: number
): string[] {
    let currentGeneration: string[] = [...symbols];

    for (let i = 0; i < generations; i++) {
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

    if (length === 0) return;

    const nx = -dy / length;
    const ny = dx / length;
    const h = thickness / 2;

    out.push(
        x1 + nx * h, y1 + ny * h,
        x1 - nx * h, y1 - ny * h,
        x2 + nx * h, y2 + ny * h,

        x2 + nx * h, y2 + ny * h,
        x1 - nx * h, y1 - ny * h,
        x2 - nx * h, y2 - ny * h
    );
}

function turtleToVertices(symbols: string[]): Float32Array {
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
                const x1 = cursor.x_position;
                const y1 = cursor.y_position;

                const steps = Number(stack.pop());
                cursor.x_position += Math.cos(cursor.direction) * steps;
                cursor.y_position += Math.sin(cursor.direction) * steps;

                const x2 = cursor.x_position;
                const y2 = cursor.y_position;

                appendLineVertices(vertices, x1, y1, x2, y2, 1);
                break;
            }

            case 'ROTATE': {
                const angle = Number(stack.pop());
                cursor.direction += (angle * Math.PI) / 180;
                break;
            }

            default:
                stack.push(symbol);
        }
    }

    return new Float32Array(vertices);
}

self.onmessage = (event: MessageEvent<WorkerRequest>) => {
    const { symbols, substitutions, generations } = event.data;

    const expanded = lindenmayer(
        symbols,
        new Map(substitutions),
        generations
    );

    const vertices = turtleToVertices(expanded);

    const response: WorkerResponse = { vertices };
    self.postMessage(response, [vertices.buffer]);
};