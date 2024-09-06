const usernameEl = document.querySelector('#nombreUsuario');
const passwordEl = document.querySelector('#Password');

const form = document.querySelector('#frmLogin');


const checkUsername = () => {

    let valid = false;

    const min = 3,
        max = 25;

    const username = usernameEl.value.trim();

    if (!isRequired(username)) {
        showError('nombreUsuario', 'El usuario no puede estar en blanco.');
    } else if (!isBetween(username.length, min, max)) {
        showError('nombreUsuario', `El nombre de usuario debe tener de ${min} and ${max} caracteres.`)
    } else {
        showSuccess('nombreUsuario');
        valid = true;
    }
    return valid;
};


//const checkEmail = () => {
//    let valid = false;
//    const email = emailEl.value.trim();
//    if (!isRequired(email)) {
//        showError(emailEl, 'Email cannot be blank.');
//    } else if (!isEmailValid(email)) {
//        showError(emailEl, 'Email is not valid.')
//    } else {
//        showSuccess(emailEl);
//        valid = true;
//    }
//    return valid;
//};

const checkPassword = () => {
    let valid = false;


    const password = passwordEl.value.trim();

    if (!isRequired(password)) {
        showError('Password', 'Password cannot be blank.');
    } else if (!isPasswordSecure(password)) {
        showError('Password', 'Password must has at least 8 characters that include at least 1 lowercase character, 1 uppercase characters, 1 number, and 1 special character in (!@#$%^&*)');
    } else {
        showSuccess('Password');
        valid = true;
    }

    return valid;
};

//const checkConfirmPassword = () => {
//    let valid = false;
//    // check confirm password
//    const confirmPassword = confirmPasswordEl.value.trim();
//    const password = passwordEl.value.trim();

//    if (!isRequired(confirmPassword)) {
//        showError(confirmPasswordEl, 'Please enter the password again');
//    } else if (password !== confirmPassword) {
//        showError(confirmPasswordEl, 'The password does not match');
//    } else {
//        showSuccess(confirmPasswordEl);
//        valid = true;
//    }

//    return valid;
//};

//const isEmailValid = (email) => {
//    const re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
//    return re.test(email);
//};

//const isPasswordSecure = (password) => {
//    const re = new RegExp("^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#\$%\^&\*])(?=.{8,})");
//    return re.test(password);
//};

const isRequired = value => value === '' ? false : true;
const isBetween = (length, min, max) => length < min || length > max ? false : true;


const showError = (input, message) => {
    $("#" + input).removeClass("valid");
    $("#" + input).addClass("input-validation-error");
    var span = $("#" + input).parent().find("span.text-danger");
    if (span.length == 0) {
        span = $("#" + input).parent().parent().find("span.text-danger");
    }
    span.removeClass("field-validation-valid");
    span.addClass("field-validation-error");
    span.empty();
    span.append('<span id=' + input + '-error" class="">' + message + '</span>');
    $("#" + input).focus();
};

const showSuccess = (input) => {
    // get the form-field element
    const formField = input.parentElement;

    // remove the error class
    formField.classList.remove('error');
    formField.classList.add('success');

    // hide the error message
    const error = formField.querySelector('small');
    error.textContent = '';
}


form.addEventListener('submit', function (e) {
    // prevent the form from submitting
    e.preventDefault();

    // validate fields
    let isUsernameValid = checkUsername(),
        //isEmailValid = checkEmail(),
        isPasswordValid = checkPassword();
        //isConfirmPasswordValid = checkConfirmPassword();

    let isFormValid = isUsernameValid &&
        //isEmailValid &&
        isPasswordValid; //&&
        //isConfirmPasswordValid;

    // submit to the server if the form is valid
    if (isFormValid) {
        form.submit();
    }
});


//const debounce = (fn, delay = 500) => {
//    let timeoutId;
//    return (...args) => {
//        // cancel the previous timer
//        if (timeoutId) {
//            clearTimeout(timeoutId);
//        }
//        // setup a new timer
//        timeoutId = setTimeout(() => {
//            fn.apply(null, args)
//        }, delay);
//    };
//};

//form.addEventListener('input', debounce(function (e) {
//    switch (e.target.id) {
//        case 'username':
//            checkUsername();
//            break;
//        case 'email':
//            checkEmail();
//            break;
//        case 'password':
//            checkPassword();
//            break;
//        case 'confirm-password':
//            checkConfirmPassword();
//            break;
//    }
//}));