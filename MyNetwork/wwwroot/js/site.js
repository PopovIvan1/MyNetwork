let siteColor = 'white';

function uploadImage() {
    $.ajax({
        url: "/MyPage/UploadImage",
        method: 'GET',
    });
}

function decodeString(str) {
    str = decodeURI(str);
    str = str.replaceAll("%2f", "/").replaceAll("%26nbsp%3b", " ");
    return str;
}

function addTagLabel(id) {
    let isEmptyTags = 0;
    let tags = document.querySelectorAll(id);
    tags.forEach(item => {
        if (item.querySelector("#tag").value == "") isEmptyTags += 1;
        if (isEmptyTags > 1 && item.querySelector("#tag").value == "") item.remove();
    });
    if (isEmptyTags == 0) {
        createNewTag();
    }
}

function checkNumber() {
    if (document.getElementById('number').value > 10) {
        document.getElementById('number').value = 10;
    }
    if (document.getElementById('number').value == '' || document.getElementById('number').value == null) {
        document.getElementById('number').value = 0;
    }
}

function createNewTag() {
    let parent = document.getElementById('tags');
    let elem = parent.querySelector('.form-floating');

    let clone = elem.cloneNode(true);
    parent.appendChild(clone);
    document.getElementById('tag').value = "";
}


function getUserRate(rate, id) {
    let rateNumber = parseFloat(rate);
    let i;
    let starRate = '';
    for (i = 0; i < 5; i++) {
        if (rateNumber >= i + 1) {
            starRate += '<i class="bi bi-star-fill"></i>';
        }
        else if (rateNumber + 0.5 >= i + 1) {
            starRate += '<i class="bi bi-star-half"></i>';
        }
        else {
            starRate += '<i class="bi bi-star"></i>';
        }
    }
    document.getElementById(id).insertAdjacentHTML("beforeend", starRate);
}

function changeRate(newRate) {
    document.getElementById('yourRate').innerHTML = '';
    getUserRate(newRate, 'yourRate');
    let stars = document.getElementById('yourRate').querySelectorAll("i");
    stars.forEach((star, i) => {
        star.onclick = function () { changeRate(i + 1) };
    })
    $.ajax({
        url: "/Review/ChangeCreationRate",
        method: 'GET',
        data: {
            rate: newRate,
            creationName: document.getElementById('creationName').innerHTML
        }
    });
}

function like() {
    let curVal = Number(document.getElementById('likesCount').innerHTML);
    let isLike = 0;
    if (document.getElementById('heart').style.color == "blue") {
        document.getElementById('heart').style.color = "";
        curVal--;
        document.getElementById('likesCount').innerHTML = curVal
    }
    else {
        document.getElementById('heart').style.color = "blue"
        curVal++;
        document.getElementById('likesCount').innerHTML = curVal;
        isLike = 1;
    }
    $.ajax({
        url: "/Review/LikeReview",
        method: 'GET',
        data: {
            likeCount: curVal,
            likeOrDislike: isLike
        }
    });
}

$(document).ready(function () {
    $("#lastDiv").nextAll("center").remove();
    $("#lastDiv").nextAll("div").remove();
    setTimeout(function () {
        $("#lastDiv").nextAll("div").remove();
    }, 500);
    setTimeout(function () {
        $("#lastDiv").nextAll("div").remove();
    }, 1000);
    setTimeout(function () {
        $("#lastDiv").nextAll("div").remove();
    }, 3000);
});

let displayMode = 'none';
function changeTheme() {
    let id;
    if (document.getElementById('menuContent').style.display == 'none') id = 'themeSelector';
    else id = 'themeMobileSelector';
    $.ajax({
        url: "/Home/ChangeTheme",
        method: 'GET',
        data: {
            theme: document.getElementById(id).value
        }
    });
    if (siteColor == 'white') {
        siteColor = 'black';
        setDark();
    }
    else {
        siteColor = 'white';
        setWhite();
    }
}

function setDark() {
    document.body.style.backgroundColor = '#151313';
    document.body.style.color = 'white';
    document.querySelectorAll('table').forEach(item => {
        item.style.color = 'white';
    })
}

function setWhite() {
    document.body.style.backgroundColor = 'white';
    document.body.style.color = 'black';
    document.querySelectorAll('table').forEach(item => {
        item.style.color = 'black';
    })
}

function changeLanguage() {
    let id;
    if (document.getElementById('menuContent').style.display == 'none') id = 'languageSelector';
    else id = 'languageMobileSelector';
    $.ajax({
        url: "/Home/ChangeLanguage",
        method: 'GET',
        data: {
            language: document.getElementById(id).value
        }
    });
    setTimeout(function () {
        location.reload();
    }, 700);
}

function displayMenu() {
    if (displayMode == 'none') {
        document.getElementById('menuContent').style.display = 'block';
        displayMode = 'block';
    }
    else {
        document.getElementById('menuContent').style.display = 'none';
        displayMode = 'none';
    }
}

let parrent = document.getElementById('menuContent');
document.getElementById('navbar').querySelectorAll('li').forEach((element, index) => {
    if (index == 0) {
        let clone = element.querySelector('a').cloneNode(true);
        parrent.appendChild(clone);
    }
    else if (index == 5) {
        let clone = element.querySelector('a').cloneNode(true);
        parrent.appendChild(clone);
    }
    else if (index == 6) {
        if (element.querySelectorAll('a').length > 0) {
            let clone = element.querySelector('a').cloneNode(true);
            parrent.appendChild(clone);
        }
        else {
            let clone = element.querySelector('form').cloneNode(true);
            parrent.appendChild(clone);
        }
    }
});

function AutoScale() {
    let width = window.innerWidth;
    let elements = document.getElementById('navbar').querySelectorAll('li');
    if (width < 900) {
        elements.forEach(element => {
            element.style.display = 'none';
        });
        document.getElementById('menu').style.display = 'block';
    }
    else {
        elements.forEach(element => {
            element.style.display = 'block';
        });
        document.getElementById('menu').style.display = 'none';
        document.getElementById('menuContent').style.display = 'none';
        displayMode = 'none';
    }
}

window.addEventListener("resize", AutoScale);
AutoScale();
