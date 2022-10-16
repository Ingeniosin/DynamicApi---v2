import {useMemo} from "react";

const captions = {
    nombre: 'Nombre', 
	displayName: 'Display Name'
}

const columns = [
    {dataField: 'nombre', dataType: 'string', caption: captions['nombre'], required: true}, 
	{dataField: 'displayName', dataType: 'string', caption: captions['displayName'], required: true}
]

const Grid = ({filter, reference}) => {
    const configuration = useMemo(() => ({
        reference,
        columns,
        editorMode: 'cell',
        dataSource: {
            api: "FormasAdquisicion",
            pageSize: 10,
            filter
        }
    }), [filter, reference]);
    return useCustomGrid(configuration)
};

export default Grid;