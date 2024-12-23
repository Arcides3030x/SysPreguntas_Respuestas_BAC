const fotoIndices = {};
const fotoIndices = {};

/**
 * Función para cambiar la foto en una tarjeta de pregunta
 * @param {number} direccion - Dirección del cambio (-1 para atrás, 1 para adelante)
 * @param {Array} fotos - Lista de fotos asociadas a la pregunta
 * @param {string} imgId - ID del elemento img
 */
function cambiarFoto(direccion, fotos, imgId) {
    if (!fotoIndices[imgId]) {
        fotoIndices[imgId] = 0; // Inicializar índice si no existe
    }

    // Calcular el nuevo índice
    fotoIndices[imgId] += direccion;

    // Controlar los límites del índice
    if (fotoIndices[imgId] < 0) {
        fotoIndices[imgId] = fotos.length - 1; // Vuelve a la última foto
    } else if (fotoIndices[imgId] >= fotos.length) {
        fotoIndices[imgId] = 0; // Vuelve a la primera foto
    }

    // Cambiar la imagen actual
    const nuevaFoto = fotos[fotoIndices[imgId]].UrlFoto;
    document.getElementById(imgId).src = nuevaFoto;
}