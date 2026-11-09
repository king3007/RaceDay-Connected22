const toggle = document.querySelector('.menu-toggle');
const menu = document.getElementById('public-menu');
if (toggle && menu) toggle.addEventListener('click', () => {
 const open = toggle.getAttribute('aria-expanded') !== 'true';
 toggle.setAttribute('aria-expanded', String(open)); menu.classList.toggle('is-open', open);
});
document.querySelectorAll('nav a[href]').forEach(link => {
 if(new URL(link.href).pathname.toLowerCase() === location.pathname.toLowerCase()) link.setAttribute('aria-current','page');
});

// Local previews only; the form submits the image file to the MVC controller.
document.querySelectorAll('[data-image-input]').forEach(input => {
 let previousUrl;
 const preview = document.getElementById(input.dataset.previewTarget);
 const feedback = input.closest('.upload-panel').querySelector('.upload-feedback');
 input.addEventListener('change', () => {
  if(previousUrl) { URL.revokeObjectURL(previousUrl); previousUrl = null; }
  preview.hidden = true; preview.removeAttribute('src'); feedback.textContent = ''; input.setCustomValidity('');
  const file = input.files[0]; if(!file) return;
  const validExtension = /\.(jpe?g|png|webp)$/i.test(file.name);
  if(!validExtension || file.size === 0 || file.size > 5 * 1024 * 1024) {
   const message = 'Choose a JPG, PNG or WebP image, up to 5 MB.';
   feedback.textContent = message; input.setCustomValidity(message); return;
  }
  previousUrl = URL.createObjectURL(file); preview.src = previousUrl; preview.hidden = false;
  feedback.textContent = 'Preview ready. Save the form to upload this image.';
 });
 input.form?.addEventListener('reset', () => {
  if(previousUrl) URL.revokeObjectURL(previousUrl);
  preview.hidden = true; preview.removeAttribute('src'); feedback.textContent = ''; input.setCustomValidity('');
 });
 window.addEventListener('pagehide', () => { if(previousUrl) URL.revokeObjectURL(previousUrl); });
});
