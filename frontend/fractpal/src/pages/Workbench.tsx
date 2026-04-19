import React, { useState, useEffect, useRef, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { fractalApi } from '../services/api';
import {
  createLSystemWorker,
  drawVertices,
  requestVertices,
  resizeCanvas,
  setupWebGL,
  type FractalRenderDefinition,
} from '../services/lsystem';
import './Workbench.css';

const Workbench: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const workerRef = useRef<Worker | null>(null);
  const renderRequestIdRef = useRef(0);

  const [name, setName] = useState('Untitled Fractal');
  const [axiom, setAxiom] = useState('F - - F - - F');
  const [rules, setRules] = useState('F = F + F - - F + F');
  const [instructions, setInstructions] = useState('F = 10 FORWARD\n+ = 60 ROTATE\n- = -60 ROTATE');
  const [generations, setGenerations] = useState(4);
  const [xTranslation, setXTranslation] = useState(0);
  const [yTranslation, setYTranslation] = useState(0);
  const [zoom, setZoom] = useState(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  // ------------------ Load Fractal ------------------
  const loadFractal = useCallback(async () => {
    if (!id) return;
    try {
      setLoading(true);
      const fractal = await fractalApi.getFractalById(id);
      setName(fractal.name);
      setAxiom(fractal.axiom);
      setRules(fractal.rules);
      setInstructions(fractal.instructions);
      setGenerations(fractal.generations);
      setXTranslation(fractal.xTranslation);
      setYTranslation(fractal.yTranslation);
      setZoom(fractal.zoom);
    } catch (err) {
      console.error('Failed to load fractal', err);
      setError('Failed to load fractal');
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => { loadFractal(); }, [loadFractal]);

  useEffect(() => {
    workerRef.current = createLSystemWorker();

    return () => {
      workerRef.current?.terminate();
      workerRef.current = null;
    };
  }, []);

  // ------------------ Render Fractal ------------------
  const renderFractal = useCallback(async () => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    try {
      // Maintain 4:3 aspect ratio
      const width = Math.min(canvas.parentElement?.clientWidth ?? 800, 800);
      const height = Math.floor((width / 4) * 3);
      canvas.style.width = `${width}px`;
      canvas.style.height = `${height}px`;
      resizeCanvas(canvas);

      const gl = setupWebGL(canvas, xTranslation, yTranslation, zoom);
      if (!gl) return;

      const definition: FractalRenderDefinition = {
        axiom,
        rules,
        instructions,
        generations,
      };
      const requestId = ++renderRequestIdRef.current;
      const vertices = await requestVertices(workerRef.current, definition, requestId);

      if (requestId !== renderRequestIdRef.current) {
        return;
      }

      drawVertices(gl, vertices);
    } catch (err) {
      console.error('Render error:', err);
    }
  }, [axiom, rules, instructions, generations, xTranslation, yTranslation, zoom]);

  useEffect(() => {
    void renderFractal();
  }, [renderFractal]);

  // ------------------ Capture Thumbnail ------------------
  const captureThumbnail = async (canvas: HTMLCanvasElement): Promise<string> => {
    await renderFractal();
    await new Promise<void>(resolve => {
      requestAnimationFrame(() => resolve());
    });

    // Draw to smaller canvas for thumbnail
    const thumb = document.createElement('canvas');
    thumb.width = 400; // fixed thumbnail size
    thumb.height = 300; // 4:3
    const ctx = thumb.getContext('2d');
    if (!ctx) return '';
    ctx.drawImage(canvas, 0, 0, thumb.width, thumb.height);
    return thumb.toDataURL('image/jpeg', 0.8);
  };

  // ------------------ Save Fractal ------------------
  const handleSave = async () => {
    try {
      setLoading(true);
      setError('');

      const canvas = canvasRef.current;
      const thumbnail = canvas ? await captureThumbnail(canvas) : undefined;

      const fractalData = {
        name,
        axiom,
        rules,
        instructions,
        generations,
        xTranslation,
        yTranslation,
        zoom,
        thumbnail,
      };

      if (id) {
        await fractalApi.updateFractal(id, fractalData);
      } else {
        const newFractal = await fractalApi.createFractal(fractalData);
        navigate(`/workbench/${newFractal.id}`);
      }
    } catch (err: any) {
      console.error(err);
      setError(err.message ?? 'Failed to save fractal');
    } finally {
      setLoading(false);
    }
  };

  const handleDiscard = () => {
    if (id) {
      navigate('/gallery');
    } else {
      setName('Untitled Fractal');
      setAxiom('F');
      setRules('F = F + F - - F + F');
      setInstructions('F = 10 FORWARD\n+ = 60 ROTATE\n- = -60 ROTATE');
      setGenerations(4);
      setXTranslation(0);
      setYTranslation(0);
      setZoom(1);
    }
  };

  return (
    <div className="workbench-page">
      <div className="workbench-header">
        <input
          type="text"
          value={name}
          onChange={e => setName(e.target.value)}
          className="fractal-name-input"
          placeholder="Fractal Name"
        />
        <div className="workbench-actions">
          <button onClick={handleDiscard} className="secondary">
            Discard
          </button>
          <button onClick={handleSave} disabled={loading} className="primary">
            {loading ? <span className="loading" /> : 'Save'}
          </button>
        </div>
      </div>

      {error && <div className="error-message">{error}</div>}

      <div className="workbench-content">
        <div className="controls-panel">
          <div className="control-section">
            <label htmlFor="axiom">Axiom</label>
            <input
              id="axiom"
              type="text"
              value={axiom}
              onChange={e => setAxiom(e.target.value)}
              placeholder="F"
            />
          </div>
          <div className="control-section">
            <label htmlFor="rules">Rules</label>
            <textarea
              id="rules"
              value={rules}
              onChange={e => setRules(e.target.value)}
              placeholder="F = F + F - - F + F"
              rows={5}
            />
          </div>
          <div className="control-section">
            <label htmlFor="instructions">Instructions</label>
            <textarea
              id="instructions"
              value={instructions}
              onChange={e => setInstructions(e.target.value)}
              placeholder="F = 10 FORWARD"
              rows={8}
            />
          </div>
          <div className="control-section">
            <label htmlFor="generations">Generations: {generations}</label>
            <input
              id="generations"
              type="range"
              min="1"
              max="10"
              value={generations}
              onChange={e => setGenerations(Number(e.target.value))}
            />
          </div>
          <div className="control-grid">
            <div className="control-section">
              <label htmlFor="xTranslation">X Translation</label>
              <input
                id="xTranslation"
                type="number"
                value={xTranslation}
                onChange={e => setXTranslation(Number(e.target.value))}
                step="10"
              />
            </div>
            <div className="control-section">
              <label htmlFor="yTranslation">Y Translation</label>
              <input
                id="yTranslation"
                type="number"
                value={yTranslation}
                onChange={e => setYTranslation(Number(e.target.value))}
                step="10"
              />
            </div>
            <div className="control-section">
              <label htmlFor="zoom">Zoom</label>
              <input
                id="zoom"
                type="number"
                value={zoom}
                onChange={e => setZoom(Number(e.target.value))}
                step="0.1"
                min="0.1"
              />
            </div>
          </div>
        </div>

        <div className="canvas-panel">
          <canvas ref={canvasRef} className="fractal-canvas" />
        </div>
      </div>
    </div>
  );
};

export default Workbench;
