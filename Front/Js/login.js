// Mostrar ventana emergente de bienvenida 
Swal.fire({
    title: '¡Bienvenido!',
    text: 'Te invito a que valides tu usuario!',
    icon: 'info',
    confirmButtonText: 'Aceptar'
});

const urlAuth = "http://localhost:5183/User/validarUser";
const headers = new Headers({ 'Content-Type': 'application/json' });
const boton = document.getElementById('boton');

boton.addEventListener("click", function (e) {
    e.preventDefault();
    autorizarUsuario();
});

async function autorizarUsuario() {
    let inputUsuario = document.getElementById('username').value;
    let inputPassword = document.getElementById('password').value;

    let data = {
        "username": inputUsuario,
        "password": inputPassword
    }

    const config = {
        method: 'POST',
        headers: headers,
        body: JSON.stringify(data)
    };

    try {
        const response = await fetch(urlAuth, config);

        if (response.ok) {
            // La autenticación fue exitosa, redirige al usuario a la página Swagger
            window.location.href = "http://localhost:5183/swagger";
        } else {
            // La autenticación falló, muestra un mensaje de error o redirige al usuario a la página de registro
            alert("Autenticación fallida. Verifique sus credenciales o regístrese.");
        }
    } catch (error) {
        console.error("Error al realizar la autenticación:", error);
    }
}