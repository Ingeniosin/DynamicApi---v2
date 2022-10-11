import {useMemo} from "react";

const captions = {
    asesor: 'Asesor', 
	usuario: 'Usuario', 
	tipoCompraVehiculo: 'Tipo Compra Vehiculo', 
	marca: 'Marca', 
	modelo: 'Modelo', 
	presupuesto: 'Presupuesto', 
	cuotaMensual: 'Cuota Mensual', 
	formaAdquisicion: 'Forma Adquisicion'
}

const columns = [
    {dataField: 'asesorId', dataType: 'number', caption: captions['asesor'], required: true, lookup: getDsLookup('Asesores', null, 'nombre')}, 
	{dataField: 'usuarioId', dataType: 'number', caption: captions['usuario'], required: true, lookup: getDsLookup('Usuarios', null, 'nombreCompleto')}, 
	{dataField: 'tipoCompraVehiculoId', dataType: 'number', caption: captions['tipoCompraVehiculo'], required: true, lookup: getDsLookup('TiposCompraVehiculo', null, 'nombre')}, 
	{dataField: 'marca', dataType: 'string', caption: captions['marca'], required: true}, 
	{dataField: 'modelo', dataType: 'string', caption: captions['modelo'], required: true}, 
	{dataField: 'presupuesto', dataType: 'string', caption: captions['presupuesto'], required: true}, 
	{dataField: 'cuotaMensual', dataType: 'string', caption: captions['cuotaMensual'], required: true}, 
	{dataField: 'formaAdquisicionId', dataType: 'number', caption: captions['formaAdquisicion'], required: true, lookup: getDsLookup('FormasAdquisicion', null, 'nombre')}
]

const Grid = ({filter, reference}) => {
    const configuration = useMemo(() => ({
        reference,
        columns,
        dataSource: {
            api: "Solicitudes",
            pageSize: 10,
            filter
        }
    }), [filter, reference]);
    return useCustomGrid(configuration)
};

export default Grid;