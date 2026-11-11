(() => {
  'use strict';

  const hero = document.querySelector('.hero-floating');
  const control = document.getElementById('alternar-animacao');
  if (!hero || !control) return;

  const preference = window.matchMedia('(prefers-reduced-motion: reduce)');
  const label = control.querySelector('span');
  const icon = control.querySelector('use');
  let userPaused = false;
  let isVisible = true;

  // As imagens só animam quando estão visíveis e o usuário permite movimento.
  const updateMotion = () => {
    const running = !preference.matches && !userPaused && isVisible && !document.hidden;
    hero.dataset.motion = running ? 'running' : 'paused';
    control.hidden = preference.matches;
    label.textContent = userPaused ? 'Retomar animação' : 'Pausar animação';
    icon.setAttribute('href', userPaused ? '#icon-play' : '#icon-pause');
  };

  control.addEventListener('click', () => {
    userPaused = !userPaused;
    updateMotion();
  });
  document.addEventListener('visibilitychange', updateMotion);
  preference.addEventListener('change', updateMotion);

  if ('IntersectionObserver' in window) {
    const observer = new IntersectionObserver(([entry]) => {
      isVisible = entry.isIntersecting;
      updateMotion();
    });
    observer.observe(hero);
  }

  updateMotion();
})();
