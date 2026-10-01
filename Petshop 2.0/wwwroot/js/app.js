const STATUS = {
  CheckInRealizado:   ['Aguardando',      'secondary'],
  ServicoIniciado:    ['Em serviço',      'warning'],
  ServicoFinalizado:  ['Pronto',          'info'],
  NotificacaoEnviada: ['Tutor avisado',   'success'],
  CheckOutRealizado:  ['Entregue',        'dark'],
};
const COR_LOG = { sucesso: 'success', retry: 'warning', erro: 'warning', dlq: 'danger' };

const pets = new Map();            
const painelOperacional = new Set();
let ultimoId = 0;
let totalDlq = 0;

const esc = s => String(s ?? '').replace(/[&<>"']/g,
  c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));

async function post(url, corpo) {
  try {
    const r = await fetch(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: corpo ? JSON.stringify(corpo) : undefined,
    });
    if (!r.ok) alert('Falha ao chamar ' + url);
  } catch { alert('Servidor indisponível.'); }
}

function adicionarLog(e) {
  const ul = document.getElementById('log');
  if (!ul) return;
  const li = document.createElement('li');
  li.className = 'list-group-item py-1 small' + (COR_LOG[e.tipo] ? ' list-group-item-' + COR_LOG[e.tipo] : '');
  li.innerHTML = `<span class="text-body-secondary">${esc(e.hora)}</span> <strong>${esc(e.origem)}</strong> ${esc(e.mensagem)}`;
  ul.prepend(li);
  while (ul.children.length > 100) ul.lastChild.remove();
}

function conectar(aoMudar) {
  const badge = document.getElementById('conexao');
  const es = new EventSource('/api/eventos');
  es.onopen = () => { badge.className = 'badge text-bg-success'; badge.textContent = 'ao vivo'; };
  es.onerror = () => { badge.className = 'badge text-bg-danger'; badge.textContent = 'reconectando...'; };
  es.onmessage = m => {
    const e = JSON.parse(m.data);
    if (e.id <= ultimoId) return;            
    ultimoId = e.id;
    if (e.tipo === 'recepcao') pets.set(e.petId, { petId: e.petId, nome: e.nomePet, email: e.emailTutor, evento: e.evento });
    if (e.tipo === 'operacional') painelOperacional.add(e.petId);
    if (e.tipo === 'dlq') {
      totalDlq++;
      const el = document.getElementById('dlq');
      if (el) el.textContent = totalDlq;
    }
    adicionarLog(e);
    aoMudar();
  };
}
