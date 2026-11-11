/* Cadastro, login e edição da conta pela API existente. */
(() => {
  'use strict'; const P=window.Pim;
  P.nextRoute = null;
  P.requireLogin = (admin=false) => {
    if (!P.user()) {
      P.nextRoute = location.hash || '#/cliente';
      location.hash = '#/conta/login';
      P.message('Entre na sua conta para continuar.','info'); return false;
    }
    if (admin && !P.admin()) throw new Error('Esta área é exclusiva da administração.');
    return true;
  };
  P.loginPage = register => {
    P.render(P.template(register ? 'cadastro' : 'login'));
    const form=document.getElementById(register ? 'cadastro-form' : 'login-form');
    P.initCpf(form);
    P.form(form, async data => {
      if (register && data.senha !== data.confirmarSenha) throw new Error('As senhas precisam ser iguais.');
      const result=await P.request('/api/conta/'+(register?'cadastro':'login'),{method:'POST',body:data});
      P.saveSession(result);
      const next=P.nextRoute; P.nextRoute=null;
      location.hash=next || (P.admin()?'#/admin':'#/cliente');
      P.message(register ? 'Conta criada. Bem-vindo à PimBurguer!' : 'Você entrou na sua conta.');
    });
  };
  P.logout = async () => {
    try { if (P.session) await P.request('/api/conta/sair',{method:'POST'}); }
    finally { P.saveSession(null); P.nextRoute=null; location.hash='#/'; P.message('Você saiu da conta.'); }
  };
  P.profilePage = async () => {
    if (!P.requireLogin()) return;
    const user=await P.request('/api/conta/me');
    P.session.usuario=user; P.saveSession(P.session);
    P.panel(P.template('perfil'), 'perfil');
    const form=document.getElementById('perfil-form');
    form.elements.nome.value=user.nome; form.elements.telefone.value=user.telefone;
    document.getElementById('perfil-identificacao').textContent=`${user.email || user.nomeUsuario || ''}${user.cpf ? ' · CPF '+user.cpf : ''}`;
    P.form(form,async data => {
      P.session.usuario=await P.request('/api/conta/perfil',{method:'PUT',body:data}); P.saveSession(P.session);
      P.message('Seus dados foram atualizados.');
    });
    P.form(document.getElementById('senha-form'),async data => {
      if(data.novaSenha!==data.confirmarSenha) throw new Error('As senhas precisam ser iguais.');
      await P.request('/api/conta/senha',{method:'PUT',body:data}); P.saveSession(null);
      P.message('Senha alterada. Entre novamente com sua nova senha.'); location.hash='#/conta/login';
    });
  };
})();
