// Mostrar ventana emergente de bienvenida 
Swal.fire({
    title: '¡Bienvenido!',
    text: 'Te invito a que te registres!',
    icon: 'info',
    confirmButtonText: 'Aceptar'
});

const urlRegister = "http://localhost:5183/User/registrar";
const headers = new Headers({ 'Content-Type': 'application/json' });
const boton = document.getElementById('botonRegistro');

boton.addEventListener("click", function (e) {
    e.preventDefault();
    registrarUsuario();
});

async function registrarUsuario() {
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
        const response = await fetch(`${urlRegister}`, config);

        if (response.status === 200) {
            window.location.href = '../index.html';
        } else {
            console.error("La solicitud no fue exitosa. Estado:", response.status);
        }
    } catch (error) {
        console.error("Error de red: ", error);
    }
}