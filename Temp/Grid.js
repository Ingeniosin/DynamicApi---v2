import {useMemo} from "react";

const captions = {
    nombre: 'Nombre',
    apellido: 'Apellido',
    email: 'Email',
    contrasena: 'Contrasena'
}

const columns = [
    {dataField: 'nombre', dataType: 'string', caption: captions['nombre'], required: true},
    {dataField: 'apellido', dataType: 'string', caption: captions['apellido'], required: true},
    {dataField: 'email', dataType: 'string', caption: captions['email'], required: true},
    {dataField: 'contrasena', dataType: 'string', caption: captions['contrasena'], required: true}
]

const Grid = ({filter, reference}) => {
    const configuration = useMemo(() => ({
        reference,
        columns,
        editorMode: 'cell',
        dataSource: {
            api: "Usuarios",
            pageSize: 10,
            filter
        }
    }), [filter, reference]);
    return useCustomGrid(configuration)
};

export default Grid;