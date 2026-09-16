// Pencil layer: a root down the left margin branching into each project row,
// and loose drafting frames around the banners. Seeded, so the drawing
// is identical on every load; measured from the live layout.
(() => {
  const NS = 'http://www.w3.org/2000/svg';
  const main = document.querySelector('main');
  if (!main) return;

  const svg = document.createElementNS(NS, 'svg');
  svg.setAttribute('class', 'pencil-layer');
  svg.setAttribute('aria-hidden', 'true');
  svg.innerHTML = `<defs>
    <filter id="graphite" x="-5%" y="-5%" width="110%" height="110%">
      <feTurbulence type="fractalNoise" baseFrequency="0.9" numOctaves="2" seed="7" result="n"/>
      <feDisplacementMap in="SourceGraphic" in2="n" scale="1.6"/>
    </filter></defs><g filter="url(#graphite)"></g>`;
  main.prepend(svg);
  const g = svg.querySelector('g');

  let seed;
  const rand = () => ((seed = (seed * 16807) % 2147483647) - 1) / 2147483646;
  const jit = (n) => (rand() - 0.5) * 2 * n;

  const line = (d, w, o) => {
    const p = document.createElementNS(NS, 'path');
    p.setAttribute('d', d);
    p.setAttribute('stroke-width', w);
    p.setAttribute('opacity', o);
    g.appendChild(p);
  };

  // wobbly polyline through points, drawn as smooth quadratic segments
  const wobble = (pts) => {
    let d = `M${pts[0][0].toFixed(1)} ${pts[0][1].toFixed(1)}`;
    for (let i = 1; i < pts.length - 1; i++) {
      const mx = (pts[i][0] + pts[i + 1][0]) / 2, my = (pts[i][1] + pts[i + 1][1]) / 2;
      d += ` Q${pts[i][0].toFixed(1)} ${pts[i][1].toFixed(1)} ${mx.toFixed(1)} ${my.toFixed(1)}`;
    }
    const l = pts[pts.length - 1];
    return d + ` L${l[0].toFixed(1)} ${l[1].toFixed(1)}`;
  };

  // a root: smooth random walk from a to b, braided strands, tapering, with rootlets
  const root = (a, b, width, bend, rootlets, strands = 1) => {
    const len = Math.hypot(b[0] - a[0], b[1] - a[1]);
    const steps = Math.max(8, Math.round(len / 14));
    const nx = -(b[1] - a[1]) / len, ny = (b[0] - a[0]) / len; // normal
    let drift = 0, vel = 0;
    const spine = [];
    for (let i = 0; i <= steps; i++) {
      const t = i / steps, s = Math.sin(t * Math.PI);
      vel = vel * 0.8 + jit(1.6); drift += vel;
      const w = drift * s;
      spine.push([a[0] + (b[0] - a[0]) * t + bend[0] * s + nx * w,
                  a[1] + (b[1] - a[1]) * t + bend[1] * s + ny * w]);
    }
    for (let k = 0; k < strands; k++) {
      const phase = rand() * 6, amp = k ? 1.5 + rand() * 2 : 0;
      const pts = spine.map(([x, y], i) => {
        const t = i / steps, o = Math.sin(t * 9 + phase) * amp * (1 - t * 0.6);
        return [x + nx * o + jit(0.5), y + ny * o + jit(0.5)];
      });
      // taper: successive strokes stop earlier, so the start reads heavier
      const base = k ? width * 0.45 : width;
      for (let s = 0; s < 3; s++) {
        const cut = Math.max(2, Math.round(pts.length * (1 - s * 0.28)));
        line(wobble(pts.slice(0, cut)), base * (0.55 + s * 0.35), k ? 0.35 : 0.5);
      }
    }
    for (let r = 0; r < rootlets; r++) {
      const i = 1 + Math.floor(rand() * (spine.length - 2));
      const [x, y] = spine[i], dir = rand() < 0.5 ? -1 : 1, l = 10 + rand() * 22, t = i / steps;
      line(wobble([[x, y], [x + dir * l * 0.45 + jit(2), y + l * 0.35], [x + dir * l * 0.8 + jit(3), y + l * 0.75 + jit(3)], [x + dir * l + jit(4), y + l * 1.1]]), 0.9 * (1 - t * 0.5), 0.45);
    }
    return spine;
  };

  // drafting frame: four overshooting lines, drawn twice
  const frame = (r) => {
    const o = 6;
    const L = r.left - o, T = r.top - o, R = r.right + o, B = r.bottom + o;
    for (let pass = 0; pass < 2; pass++) {
      const e = () => 3 + rand() * 6;
      line(`M${L - e()} ${T + jit(1)} L${R + e()} ${T + jit(1)}`, 0.7, pass ? 0.25 : 0.45);
      line(`M${L - e()} ${B + jit(1)} L${R + e()} ${B + jit(1)}`, 0.7, pass ? 0.25 : 0.45);
      line(`M${L + jit(1)} ${T - e()} L${L + jit(1)} ${B + e()}`, 0.7, pass ? 0.25 : 0.45);
      line(`M${R + jit(1)} ${T - e()} L${R + jit(1)} ${B + e()}`, 0.7, pass ? 0.25 : 0.45);
    }
  };

  const draw = () => {
    g.replaceChildren();
    seed = 20260916;
    const box = main.getBoundingClientRect();
    svg.setAttribute('width', box.width);
    svg.setAttribute('height', main.scrollHeight);
    svg.setAttribute('viewBox', `0 0 ${box.width} ${main.scrollHeight}`);
    if (innerWidth < 672) return; // phones: text stacks, no gutter to draw in

    const rel = (el) => {
      const r = el.getBoundingClientRect();
      return { left: r.left - box.left, right: r.right - box.left, top: r.top - box.top, bottom: r.bottom - box.top };
    };

    document.querySelectorAll('.software-project__visual, .built-plate img').forEach((el) => frame(rel(el)));

    const rows = [...document.querySelectorAll('.software-project')];
    if (!rows.length) return;
    const copies = rows.map((r) => rel(r.querySelector('.software-project__copy')));
    if (copies[0].left < 160) return; // no left margin to grow roots in
    const gutterX = copies[0].left - 64;

    // trunk: starts at the section heading and runs down the left margin
    const heading = rel(document.querySelector('#work .section-heading'));
    const last = copies[copies.length - 1];
    root([gutterX + 4, heading.top + 4], [gutterX + 10, last.top + 16], 2.2, [-18, 0], 18, 2);

    // one branch into each row, ending just before the title
    copies.forEach((c, i) => {
      const y = c.top + 14 + i * 0.5;
      const from = [gutterX + jit(3), y - 34 - rand() * 12];
      const tip = root(from, [c.left - 16, y + 4], 1.6, [10, 18], 4, 2);
      const [tx, ty] = tip[tip.length - 1]; // small curl at the tip
      line(`M${tx} ${ty} q7 1 8 -5 q0 -5 -5 -4`, 0.7, 0.5);
    });

  };

  let t;
  const redraw = () => { clearTimeout(t); t = setTimeout(draw, 60); };
  addEventListener('resize', redraw);
  document.fonts?.ready.then(redraw);
  addEventListener('load', redraw);
  draw();
})();
