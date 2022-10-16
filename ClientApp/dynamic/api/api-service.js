import {createStore} from 'devextreme-aspnet-data-nojquery';
import ArrayStore from "devextreme/data/array_store";
import DataSource from "devextreme/data/data_source";
import {toast} from "react-hot-toast";

const apiUrl = '/api/';
let token = localStorage.getItem('token') || '';

export const setToken = (t) => {
    token = t;
    localStorage.setItem('token', t);
}

export const getToken = () => token;


export const getDsOptions = (url, options = {}) => {
    options.pageSize = options.pageSize || options.take;
    return new DataSource({
        store: getDs(url.toLowerCase(), options),
        ...options
    });
}

export const getDsOptionsLookup = (url, options) => {
    return {
        store: getDs(url.toLowerCase(), options),
        ...options
    }
}

export const getDsLookupForm = (api, options  = {}, display = 'nombre', key = 'id') => {
    options = {...{
            select: [key, display],
            valueExpr: key,
            displayExpr: display,
            paginate: true,
            pageSize: 8,
            searchExpr: display,
            searchPlaceholder: 'Buscar...',
            searchMode: 'contains',
            searchEnabled: true,
            searchTimeout: 10,
            ...options
        }}

    return {
        dataSource: getDsOptions(api, {
            ...options,
        }),
        ...options,
    };
}

export const getDsLookup = (api, options, display = 'nombre', key = 'id') => {
    options = options || {};
    options = {
        select: [key, display],
        valueExpr: key,
        displayExpr: display,
        paginate: true,
        pageSize: 8,
        searchExpr: display,
        searchPlaceholder: 'Buscar...',
        searchMode: 'contains',
        searchEnabled: true,
        searchTimeout: 50,
        ...options
    }

    return {
        dataSource: getDsOptionsLookup(api, {
            ...options,
        }),
        ...options,
    };
}


export const getDsLookupArray = (arrayStore, options  = {}, display = 'nombre', key = 'id', ) => {
    options = {...{
            select: [key, display],
            valueExpr: key,
            displayExpr: display,
            paginate: true,
            pageSize: 8,
            searchExpr: display,
            searchPlaceholder: 'Buscar...',
            searchMode: 'contains',
            searchEnabled: true,
            searchTimeout: 10,
            ...options
        }}

    return {
        dataSource: arrayStore,
        ...options,
    };
}

export const getArray = (array, pageSize = 8, key = "Id") => {
    return {
        store: new ArrayStore({
            key: array.length === 0 ? null :  typeof array[0] === 'string' ? null : key,
            data: array,
        }),
        paginate: true,
        pageSize: pageSize,
    }
}

export const getDs = (api, customOptions = {}) => {
    const url = apiUrl + api.toLowerCase();
    return createStore({
        key:  'id',
        loadUrl: url,
        insertUrl: url,
        updateUrl: url,
        deleteUrl: url,
        onBeforeSend: (operation, ajaxSettings) => {
            ajaxSettings.headers = {
                'Authorization': token
            };
        },
        ...customOptions,
    });
};

export const insertFile = async (api, values, file) => {
    let formData = new FormData();
    formData.append("file", file, file.name);
    formData.append("values", JSON.stringify(values));
    let response = await fetch('/api/'+api.toString().toLowerCase(), {method: 'POST', body: formData,});
    return await response.json();
};

export const insertFiles = async (api, values, files) => {
    let formData = new FormData();
    console.log(files)
    files.forEach((file, index) => {
        formData.append("file["+index+"]", file, file.name);
    });
    formData.append("values", JSON.stringify(values));
    let response = await fetch('/api/'+api.toString().toLowerCase(), {method: 'POST', body: formData,});
    return await response.json();
};

export const insertFiles2 = async (api, values, objFiles) => {
    let formData = new FormData();
    Object.keys(objFiles).forEach((key, index) => {
        const file = objFiles[key];
        formData.append(key, file, file.name);
    });
    formData.append("values", JSON.stringify(values));
    let response = await fetch('/api/'+api.toString().toLowerCase(), {method: 'POST', body: formData,});
    return await response.json();
};

export const downloadFile = async (values) => {
    await toast.promise(downloadFileBack(values), {
        error: "Error al descargar el archivo!",
        success: "Archivo descargado correctamente!",
        loading: "Descargando archivo...",
    });
}

const downloadFileBack = async (values, modo = "ruta") => {
    let formData = new FormData();
    values.modo = modo;
    formData.append("values", JSON.stringify(values));
    let response = await fetch('/api/downloadFile', {method: 'POST', body: formData,})
    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = response.headers.get('Content-Disposition').split(';')[1].split('=')[1].replace(/\"/g, '');
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
};
