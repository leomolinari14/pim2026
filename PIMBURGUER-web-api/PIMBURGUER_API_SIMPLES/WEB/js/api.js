/* Comunicação HTTP, sessão JWT e utilitários compartilhados. Sem frameworks. */
(() => {
  'use strict';
  const P = window.Pim = {};
  P.apiBase = window.PIM_CONFIG.apiUrl.replace(/\/$/, '');
  P.read = (storage, key, fallback = null) => {
    try { return JSON.parse(storage.getItem(key)) ?? fallback; } catch { return fallback; }
  };
  P.session = P.read(sessionStorage, 'pim-acesso');
  P.user = () => P.session?.usuario;
  P.admin = () => P.user()?.perfil === 'Administrador';
  P.saveSession = value => {
    P.session = value;
    if (value) sessionStorage.setItem('pim-acesso', JSON.stringify(value));
    else sessionStorage.removeItem('pim-acesso');
    P.updateHeader?.();
  };
  if (P.session && new Date(P.session.expiraEm) <= new Date()) P.saveSession(null);
  P.escape = value => String(value ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
  P.money = value => new Intl.NumberFormat('pt-BR', {style:'currency',currency:'BRL'}).format(Number(value || 0));
  P.date = value => value ? new Date(value.length === 10 ? value+'T12:00:00' : value).toLocaleString('pt-BR', value.length === 10 ? {dateStyle:'short'} : {dateStyle:'short',timeStyle:'short'}) : '—';
  P.image = value => {
    const path = String(value ?? '');
    if (/^\/imagens\/[a-zA-Z0-9_.-]+$/.test(path)) return path.slice(1);
    if (/^\/(midia\/produtos|api\/imagens)\/[a-f0-9]{32}\.(png|jpg|webp)$/.test(path)) return P.apiBase+path;
    return 'imagens/pimburguer-simbolo.svg';
  };
  P.icon = name => `<svg class="icon" aria-hidden="true"><use href="#icon-${name}"></use></svg>`;
  P.request = async (path, options = {}) => {
    const navigation=P.navigationId;
    const controller = new AbortController();
    const timer = setTimeout(() => controller.abort(), 20000);
    const headers = new Headers(options.headers);
    if (P.session) headers.set('Authorization', 'Bearer '+P.session.token);
    let body = options.body;
    if (body !== undefined && !(body instanceof FormData)) {
      headers.set('Content-Type','application/json'); body = JSON.stringify(body);
    }
    try {
      const response = await fetch(P.apiBase+path, {...options,body,headers,signal:controller.signal,cache:'no-store'});
      const text = await response.text();
      let data;
      try { data = text ? JSON.parse(text) : null; } catch { data = null; }
      if (!response.ok) {
        if (response.status === 401 && P.session) P.saveSession(null);
        const message = response.status === 401 ? 'Sua sessão expirou ou os dados de acesso estão incorretos. Entre novamente.'
          : response.status === 403 ? 'Você não tem permissão para esta ação.'
          : data?.detail || (data?.errors ? Object.values(data.errors).flat().join(' ') : null)
          || (response.status === 429 ? 'Muitas tentativas. Aguarde alguns minutos e tente novamente.' : data?.title)
          || 'Não foi possível concluir a solicitação.';
        const error = new Error(message); error.status = response.status; throw error;
      }
      if ((!options.method || options.method === 'GET') && navigation !== undefined && navigation !== P.navigationId) {
        const error = new Error('Navegação alterada.'); error.navigation = true; throw error;
      }
      return data;
    } catch (error) {
      if (error.name === 'AbortError' || error instanceof TypeError) {
        throw new Error('Não foi possível conectar à loja. Confira se a API e o PostgreSQL estão funcionando.');
      }
      throw error;
    } finally { clearTimeout(timer); }
  };
  P.message = (text, kind = 'success') => {
    const box = document.getElementById('mensagens');
    box.hidden = false;
    box.innerHTML = `<div class="alert alert-${kind}"><span>${P.escape(text)}</span><button type="button" aria-label="Fechar aviso">×</button></div>`;
    box.querySelector('button').onclick = () => { box.hidden = true; };
  };
  P.form = (form, action) => {
    form.addEventListener('submit', async event => {
      event.preventDefault();
      const errorBox = form.querySelector('.form-error');
      if (errorBox) errorBox.hidden = true;
      if (!form.reportValidity()) return;
      const buttons = [...form.querySelectorAll('button[type="submit"],button:not([type])')];
      buttons.forEach(button => { button.disabled = true; });
      form.setAttribute('aria-busy','true');
      try { await action(Object.fromEntries(new FormData(form)), form); }
      catch (error) {
        if (errorBox) { errorBox.textContent = error.message; errorBox.hidden = false; errorBox.scrollIntoView({block:'nearest'}); }
        else P.message(error.message,'danger');
      } finally {
        buttons.forEach(button => { button.disabled = false; });
        form.removeAttribute('aria-busy');
      }
    });
  };
  P.template = id => document.getElementById('tela-'+id).innerHTML;
  P.heading = (title, text = '', action = '') => `<div class="panel-heading"><div><p class="eyebrow">${P.admin() ? 'Gestão da loja' : 'Sua conta'}</p><h1>${P.escape(title)}</h1>${text ? `<p>${P.escape(text)}</p>` : ''}</div>${action}</div>`;
  P.empty = (text, extra = '') => `<div class="panel-empty"><h2>${P.escape(text)}</h2>${extra}</div>`;
  P.metrics = entries => `<div class="metrics-grid">${entries.map(([label,value]) => `<div class="metric-card"><span>${P.escape(label)}</span><strong>${P.escape(value)}</strong></div>`).join('')}</div>`;
  P.pill = (text, kind = '') => `<span class="status-pill ${kind}">${P.escape(text)}</span>`;
  P.table = (head, rows, cls = '') => `<div class="table-responsive"><table class="table panel-table ${cls}"><thead><tr>${head.map(x=>`<th scope="col">${P.escape(x)}</th>`).join('')}</tr></thead><tbody>${rows.join('')}</tbody></table></div>`;
  P.fields = (fields, values = {}) => fields.map(([name,label,type='text',extra='']) => {
    if (type === 'checkbox') return `<label class="form-check"><input class="form-check-input" type="checkbox" name="${name}" ${values[name] ? 'checked' : ''}> ${label}</label>`;
    const value = P.escape(values[name] ?? '');
    if (type === 'textarea') return `<label>${label}<textarea class="form-control" name="${name}" ${extra}>${value}</textarea></label>`;
    return `<label>${label}<input class="form-control" type="${type}" name="${name}" ${type === 'file' ? '' : `value="${value}"`} ${extra}></label>`;
  }).join('');
  P.cpfValid = value => {
    const digits = value.replace(/\D/g,'');
    if (digits.length !== 11 || /^(.)\1{10}$/.test(digits)) return false;
    for (let size=9;size<=10;size++) {
      let sum=0; for(let i=0;i<size;i++) sum += Number(digits[i])*(size+1-i);
      const r=sum%11; if (Number(digits[size]) !== (r<2?0:11-r)) return false;
    }
    return true;
  };
  P.initCpf = root => root.querySelectorAll('[data-cpf]').forEach(field => {
    const update = () => {
      const digits = field.value.replace(/\D/g,'').slice(0,11);
      field.value = digits.replace(/^(\d{3})(\d)/,'$1.$2').replace(/^(\d{3})\.(\d{3})(\d)/,'$1.$2.$3').replace(/(\d{3})\.(\d{3})\.(\d{3})(\d)/,'$1.$2.$3-$4');
      field.setCustomValidity(P.cpfValid(digits) ? '' : 'Informe um CPF válido, com 11 dígitos.');
    };
    field.addEventListener('input', update); field.addEventListener('change', update); update();
  });
})();
