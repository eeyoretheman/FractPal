/// <reference lib="webworker" />
export { };

declare const self: DedicatedWorkerGlobalScope;

import { generateVertices, type WorkerRequest, type WorkerResponse } from './lsystem-core';

self.onmessage = (event: MessageEvent<WorkerRequest>) => {
  const { requestId, definition } = event.data;

  try {
    const vertices = generateVertices(definition);
    const response: WorkerResponse = { requestId, vertices };
    self.postMessage(response, [vertices.buffer]);
  } catch (error) {
    const response: WorkerResponse = {
      requestId,
      error: error instanceof Error ? error.message : 'Failed to generate fractal vertices.',
    };
    self.postMessage(response);
  }
};
