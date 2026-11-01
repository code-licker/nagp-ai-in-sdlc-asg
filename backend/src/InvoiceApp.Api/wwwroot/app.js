(() => {
  const money = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' });
  const $ = (id) => document.getElementById(id);
  let session = JSON.parse(sessionStorage.getItem('session') || 'null');

  function el(tag, attrs = {}, ...children) {
    const node = document.createElement(tag);
    for (const [key, value] of Object.entries(attrs)) {
      if (key === 'class') node.className = value;
      else node.setAttribute(key, value);
    }
    for (const child of children) node.append(child);
    return node;
  }

  async function api(path, options = {}) {
    const headers = { 'Content-Type': 'application/json', ...(options.headers || {}) };
    if (session) headers.Authorization = `Bearer ${session.token}`;
    const response = await fetch(`/api${path}`, { ...options, headers });
    if (response.status === 401 && session) { signOut(); throw new Error('Session expired.'); }
    const body = response.status === 204 ? null : await response.json().catch(() => null);
    if (!response.ok) throw new Error((body && body.error) || `Request failed (${response.status})`);
    return body;
  }

  function show(id, visible) { $(id).hidden = !visible; }
  function showError(id, message) { const p = $(id); p.textContent = message || ''; p.hidden = !message; }

  function render() {
    const signedIn = !!session;
    show('login-view', !signedIn);
    show('app-view', signedIn);
    show('session-bar', signedIn);
    if (signedIn) {
      $('whoami').textContent = `${session.username} (${session.role})`;
      refresh();
    }
  }

  async function refresh() {
    const invoices = await api('/invoices');
    renderInvoices(invoices);
    renderPending(invoices.filter((i) => i.status === 'PendingApproval'));
  }

  function renderInvoices(invoices) {
    const body = $('invoices-table').querySelector('tbody');
    body.replaceChildren(...invoices.map((invoice) => {
      const actions = el('td');
      if (invoice.status === 'Draft') {
        const submit = el('button', { type: 'button', class: 'secondary', 'aria-label': `Submit invoice ${invoice.id} for approval` }, 'Submit for approval');
        submit.addEventListener('click', async () => { await api(`/invoices/${invoice.id}/submit`, { method: 'POST' }); refresh(); });
        actions.append(submit);
      }
      return el('tr', {},
        el('td', {}, invoice.id),
        el('td', {}, invoice.customerName),
        el('td', {}, money.format(invoice.amount)),
        el('td', { class: `status status-${invoice.status}` }, invoice.status.replace(/([a-z])([A-Z])/g, '$1 $2')),
        actions);
    }));
  }

  function renderPending(pending) {
    show('pending-empty', pending.length === 0);
    $('pending-list').replaceChildren(...pending.map((invoice) => {
      const reasonId = `reason-${invoice.id}`;
      const approve = el('button', { type: 'button', 'aria-label': `Approve invoice ${invoice.id}` }, 'Approve');
      const reject = el('button', { type: 'button', class: 'danger', 'aria-label': `Reject invoice ${invoice.id}` }, 'Reject');
      const reason = el('input', { id: reasonId, 'aria-label': `Rejection reason for ${invoice.id}`, placeholder: 'Reason (required to reject)' });

      approve.addEventListener('click', () => decide(`/approvals/${invoice.id}/approve`, {}));
      reject.addEventListener('click', () => decide(`/approvals/${invoice.id}/reject`, { reason: reason.value }));

      return el('li', {},
        el('span', {}, `${invoice.id} · ${invoice.customerName} · ${money.format(invoice.amount)}`),
        el('span', {}, approve),
        el('div', { class: 'reject-row' }, reason, reject));
    }));
  }

  async function decide(path, payload) {
    showError('decision-error', '');
    try {
      await api(path, { method: 'POST', body: JSON.stringify(payload) });
      await refresh();
    } catch (error) {
      showError('decision-error', error.message);
    }
  }

  function signOut() {
    if (session) api('/auth/logout', { method: 'POST' }).catch(() => {});
    session = null;
    sessionStorage.removeItem('session');
    render();
  }

  $('login-form').addEventListener('submit', async (event) => {
    event.preventDefault();
    showError('login-error', '');
    try {
      const result = await api('/auth/login', {
        method: 'POST',
        body: JSON.stringify({ username: $('username').value, password: $('password').value })
      });
      session = { token: result.token, username: result.username, role: result.role };
      sessionStorage.setItem('session', JSON.stringify(session));
      $('password').value = '';
      render();
    } catch (error) {
      showError('login-error', error.message);
    }
  });

  $('new-invoice-form').addEventListener('submit', async (event) => {
    event.preventDefault();
    showError('create-error', '');
    try {
      await api('/invoices', {
        method: 'POST',
        body: JSON.stringify({ customerName: $('customer').value, amount: Number($('amount').value) })
      });
      $('new-invoice-form').reset();
      refresh();
    } catch (error) {
      showError('create-error', error.message);
    }
  });

  $('sign-out').addEventListener('click', signOut);

  for (const tab of ['invoices', 'approvals']) {
    $(`tab-${tab}`).addEventListener('click', () => {
      for (const other of ['invoices', 'approvals']) {
        $(`tab-${other}`).setAttribute('aria-selected', String(other === tab));
        show(`${other}-panel`, other === tab);
      }
    });
  }

  render();
})();
