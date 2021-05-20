let tableArray = [];
let todoContainer;
let length = 0;

const getNewInfo = async () => {
  todoContainer = await fetch("http://localhost:5000/todos").then((response) =>
    response.json()
  );
};
const somefunc = async () => {
  todoContainer = await fetch("http://localhost:5000/todos").then((response) =>
    response.json()
  );
  if (todoContainer.container) {
    renderForm();
  }
};
somefunc();

const renderForm = () => {
  document.getElementById("container").innerHTML = `
  <div class="container">
    <h1>Регистрация</h1>
    <p>Пожалуйста заполните форму чтобы зарегистрироваться или войти</p>
    <hr>

    <label for="login"><b>Логин</b></label>
    <input id="login" type="text" placeholder="Введите логин" name="login" >

    <label for="psw"><b>Пароль</b></label>
    <input id="psw" type="password" placeholder="Введите пароль" name="psw" >

    <button onclick="register()" type="submit" class="registerbtn">Зарегирстрироваться</button>
  </div>

  <div class="container signin">
    <p>Уже зарегистрированы? <button onclick="signin()">Войти</button>.</p>
  </div>
`;
};

const register = async () => {
  await getNewInfo();

  let login = document.getElementById("login").value;
  let password = document.getElementById("psw").value;
  let randomId = Math.round(Math.random() * (9999 - 1000) + 1000);
  if (todoContainer.container.users.find((item) => item.login === login)) {
    return alert("Такой логин уже есть!");
  }
  await fetch("http://localhost:5000/todos", {
    method: "post",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      id: randomId,
      login: login,
      password: password,
    }),
  }).then((response) => {
    if (response) {
      alert("Ваш логин: " + document.getElementById("login").value);
      getNewInfo();
    }
  });
};

const signin = async () => {
  let login = document.getElementById("login").value;
  let password = document.getElementById("psw").value;
  let user = todoContainer.container.users.filter((item) => {
    return item.login === login;
  });
  await getNewInfo();
  console.log(todoContainer.container.users);
  if (user[0].password === password) {
    document.getElementById("container").innerHTML = `
  <div class="content">
   <div class="navbar">
   <div class="constructorForm">
      <h2>КОНСТРУКТОР</h2>
       Количество рядов: <input class="input7" id="row" />
       Количество колонок: <input class="input7" id="column" />
        <button class="button7" onclick="getInfo()">Click me</button>
    </div>
    <div class="finderForm">
      <h2>ПОИСК</h2>
      <input class="input7" id="finder"/>
      <button class="button7" onclick="getNewCrossword()">Найти</button>
    </div>
    <div class="save">
    <div id="randomId"></div>
    <button class="button7" onclick="saveCrossword()">Сохранить</button>
    </div>
    
    </div>
    
    <main>
      <table id="table"></table>
      
    </main>
    </div>`;
    createTable(10, 10);
  } else {
    alert("Неправильный пароль");
  }
};

const saveCrossword = () => {
  let randomId = Math.round(Math.random() * (9999 - 1000) + 1000);
  document.getElementById("randomId").innerHTML =
    "Твой ID кроссворда: " + String(randomId);
  pushCrossword(randomId);
};
const pushCrossword = async (id) => {
  await fetch("http://localhost:5000/todos", {
    method: "put",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      id: id,
      crossword: tableArray,
    }),
  }).then((response) => {
    if (response) {
      alert("Ваш кроссворд сохранен");
      getNewInfo();
    }
  });
};
const getNewCrossword = () => {
  let finder = document.getElementById("finder").value;
  let newCrossword = todoContainer.container.crosswords.filter((item) => {
    return item.id == finder;
  });
  tableArray = newCrossword[0].crossword;
  renderReadyTable();
};

const renderReadyTable = () => {
  let table = document.getElementById("table");
  let trArray = "";
  tableArray.forEach((tr, indexMain) => {
    let elem = "";
    tr.forEach((td, index) => {
      let classTd = "";

      if (!td) {
        classTd = "nothing";
        elem =
          elem +
          `<td width=${length + "px"} height=${
            length + "px"
          }   class=${classTd} id=${indexMain * 100 + index}>${td}</td>`;
      } else if (td.length == 1) {
        classTd = "word";
        elem =
          elem +
          `<td width=${length + "px"} height=${
            length + "px"
          }  class=${classTd} id=${indexMain * 100 + index}>${td}</td>`;
      } else if (td == null) {
        return;
      } else {
        classTd = "quetion";

        elem =
          elem +
          `<td width=${length + "px"} height=${
            length + "px"
          }  class=${classTd} id=${indexMain * 100 + index}>${td}</td>`;
      }
    });
    trArray = trArray + `<tr>${elem}</tr>`;
  });
  table.innerHTML = trArray;
};
const renderTable = () => {
  let table = document.getElementById("table");
  let trArray = "";
  tableArray.forEach((tr, indexMain) => {
    let elem = "";
    tr.forEach((td, index) => {
      let classTd = "";
      if (!td) {
        classTd = "nothing";
        elem =
          elem +
          `<td width=${length + "px"} height=${
            length + "px"
          }  onclick="addValue()" class=${classTd} id=${
            indexMain * 100 + index
          }>${td}</td>`;
      } else if (td.length == 1) {
        classTd = "word";
        elem =
          elem +
          `<td width=${length + "px"} height=${
            length + "px"
          }  onclick="addValue()" class=${classTd} id=${
            indexMain * 100 + index
          }>${td}</td>`;
      } else if (td == null) {
        return;
      } else {
        classTd = "quetion";

        elem =
          elem +
          `<td width=${length + "px"} height=${
            length + "px"
          }  onclick="addValue()" class=${classTd} id=${
            indexMain * 100 + index
          }>${td}</td>`;
      }
    });
    trArray = trArray + `<tr>${elem}</tr>`;
  });
  table.innerHTML = trArray;
};

const createTable = (rowsCount, columnsCount) => {
  tableArray = [];
  if (rowsCount > 70 || columnsCount > 70) {
    return alert("Не больше 70 строк или столбцов");
  }
  for (i = 0; i < rowsCount; i++) {
    tableArray.push([]);
  }
  for (i = 0; i < columnsCount; i++) {
    tableArray.map((item) => item.push(""));
  }
  if (rowsCount > columnsCount) {
    length = 700 / rowsCount;
  } else {
    length = 700 / columnsCount;
  }
  renderTable();
};

const getInfo = () => {
  let rows = Number(document.getElementById("row").value);
  let columns = Number(document.getElementById("column").value);
  if (rows !== rows) {
    alert("Enter correct value");
  } else if (columns !== columns) {
    alert("Enter correct value");
  } else {
    createTable(rows, columns);
  }
};

const addValue = () => {
  let value = prompt("Введите букву или вопрос");

  if (!value || !(Number(value) !== Number(value))) {
    return alert("Некоректное значение");
  }
  tableArray.map((item, index) => {
    if (index === Math.floor(event.target.id / 100)) {
      item.map((any, index) => {
        if (index === event.target.id % 100) {
          return (item[index] = value);
        }
      });
    }
  });
  renderTable();
};
