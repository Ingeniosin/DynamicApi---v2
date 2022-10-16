import React, {memo, useEffect, useMemo, useRef, useState} from 'react';
import CustomForm from "./CustomForm";
import {ButtonItem} from "devextreme-react/form";
import {toast} from "react-hot-toast";
import LoadingSpinner from "./LoadingSpinner";

const StoredForm = ({ defaultKey, store, onSubmit, onPreSubmit, children, formReference = {}, ...props}) => {
    const formRef = useRef();
    const [data, setData] = useState(null);
    useEffect(() => {
        (async () => {
            if(!defaultKey) {
                setData({});
                return;
            }
            const item = await store.byKey(defaultKey);
            setData(item || {});
        })();
    }, [])

    const submitBtnOptions = useMemo(() => ({
        text: "Actualizar",
        type: "default",
        onClick: async () => {
            const {isValid, instance, getData, setData} = formRef.current;
            const data = getData();
            if (onPreSubmit) await onPreSubmit(data);
            if (!isValid()){
                toast.error("Los datos ingresados no son válidos.");
                return;
            }
            instance.option("disabled", true);
            toast.loading("Enviando datos...");
            try {
                const responseData = data.id && data.id !== 0 ? await store.update(data.id, data) : await store.insert(data);
/*
                setData({...data, ...responseData});
*/
                if(onSubmit) await onSubmit(responseData);
                toast.success("Datos actualizados con éxito.");
            } catch (e) {
                toast.success("Ocurrió un error al enviar los datos: " + e.message);
            }
            instance.option("disabled", false);
            formReference.current.isSubmitted = true;
        }
    }), [])
    if (!data) return <div className="m-5 d-flex flex-column align-items-center justify-content-center"><LoadingSpinner/></div>
    return (
        <CustomForm formOptions={{...props, defaultFormData: data}} reference={[formRef, formReference]}>
            {children}
            <ButtonItem buttonOptions={submitBtnOptions}/>
        </CustomForm>
    );
};

export default memo(StoredForm);
