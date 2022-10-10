import {ButtonItem, RequiredRule, SimpleItem} from "devextreme-react/form";
import {useCallback, useRef} from "react";
import {toast} from "react-hot-toast";

const captions = {
    required: 'El campo es requerido.',
    usuario: 'Usuario', 
	tipoCompraVehiculo: 'Tipo Compra Vehiculo', 
	marca: 'Marca', 
	modelo: 'Modelo', 
	presupuesto: 'Presupuesto', 
	cuotaMensual: 'Cuota Mensual', 
	formaAdquisicion: 'Forma Adquisicion'
}

const labels = {
    usuario: {text: captions['usuario']}, 
	tipoCompraVehiculo: {text: captions['tipoCompraVehiculo']}, 
	marca: {text: captions['marca']}, 
	modelo: {text: captions['modelo']}, 
	presupuesto: {text: captions['presupuesto']}, 
	cuotaMensual: {text: captions['cuotaMensual']}, 
	formaAdquisicion: {text: captions['formaAdquisicion']}
}

const messages = {
    loading: 'Guardando registro...',
    success: 'Registro enviado correctamente',
    error: e => 'Ocurrio un error al enviar el registro ' + e
}

const Form = () => {
    const formRef = useRef(null)

    const submit = useCallback(async () => {
        const {isValidAsync, getData} = formRef.current;
        const isValid = await isValidAsync();
        if(!isValid) return;
        const data = getData();
        const ds = getDs("Solicitudes");
        await toast.promise(ds.insert(data), messages);
    }, [])

    return (
        <CustomForm onEnterKey={submit} reference={formRef} formOptions={{colCount: 5}}>
            <SimpleItem dataField='usuarioId' label={labels['usuario']} editorType='dxSelectBox' editorOptions={getDsLookupForm('Usuarios', null, 'NombreCompleto')}>
				<RequiredRule message={captions['required']} />
			</SimpleItem>
			<SimpleItem dataField='tipoCompraVehiculoId' label={labels['tipoCompraVehiculo']} editorType='dxSelectBox' editorOptions={getDsLookupForm('TiposCompraVehiculo', null, 'Nombre')}>
				<RequiredRule message={captions['required']} />
			</SimpleItem>
			<SimpleItem dataField='marca' label={labels['marca']} editorType='dxTextBox'>
				<RequiredRule message={captions['required']} />
			</SimpleItem>
			<SimpleItem dataField='modelo' label={labels['modelo']} editorType='dxTextBox'>
				<RequiredRule message={captions['required']} />
			</SimpleItem>
			<SimpleItem dataField='presupuesto' label={labels['presupuesto']} editorType='dxTextBox'>
				<RequiredRule message={captions['required']} />
			</SimpleItem>
			<SimpleItem dataField='cuotaMensual' label={labels['cuotaMensual']} editorType='dxTextBox'>
				<RequiredRule message={captions['required']} />
			</SimpleItem>
			<SimpleItem dataField='formaAdquisicionId' label={labels['formaAdquisicion']} editorType='dxSelectBox' editorOptions={getDsLookupForm('FormasAdquisicion', null, 'Nombre')}>
				<RequiredRule message={captions['required']} />
			</SimpleItem>
            <ButtonItem horizontalAlignment="center" buttonOptions={{text: 'Guardar', type: 'default', onClick: submit}}/>
        </CustomForm>
    );
};

export default Form;
