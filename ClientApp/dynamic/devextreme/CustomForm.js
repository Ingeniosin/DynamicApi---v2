import Form from "devextreme-react/form";
import set from "lodash.set";
import React, {memo, useCallback} from 'react';
import {toast} from "react-hot-toast";

const setReference = (reference, instance) => {
    reference.current = ({
        instance: instance,
        getData: () => {
            const formData = {...instance.option('formData')};
            for (const key in formData) {
                if (formData[key] === null || formData[key] === undefined) {
                    delete formData[key];
                } else if (key.includes("_")) {
                    set(formData, key.replaceAll("_", "."), formData[key]);
                    delete formData[key];
                }
            }
            return formData;
        },
        setData: (data) => instance.option('formData', data),
        isValid: () => {
            const valid = instance.validate().isValid;
            if (!valid) toast.error('Formulario invalido');
            return valid
        },
        isValidAsync: async () => {
            const toastId = toast.loading('Validando formulario...');
            return new Promise((resolve, reject) => {
                setTimeout(() => {
                    setImmediate(async () => {
                        const validator = instance.validate();
                        const {isValid} = validator.complete ? await validator.complete.done() : validator;
                        if (isValid) {
                            toast.remove(toastId);
                        } else {
                            toast.error('Formulario invalido', {
                                id: toastId,
                            });
                        }
                        resolve(isValid);
                    });
                }, 25);
            })
        },
        get: (key) => instance.getEditor(key),
    })
}

const CustomForm = ({
                        children,
                        reference,
                        formOptions,
                        onEnterKey,
                        onFormDataChange,
                        defaultFormData,
                        onInit = () => {
                        },
                        logInstance
                    }) => {
    const onInitialized = useCallback(async ({component: instance}) => {
        if (reference instanceof Array) reference.filter(x => x).forEach(ref => setReference(ref, instance));
        else setReference(reference, instance);
        await onInit(reference);
        if (logInstance)
            console.log(instance)
    }, []);
    const onFieldDataChanged = useCallback(() => {
        if (onFormDataChange) onFormDataChange((oldData) => ({...oldData, ...reference.current.instance.option('formData')}))
    }, []);

    return (
        <Form  {...formOptions} onFieldDataChanged={onFieldDataChanged} defaultFormData={defaultFormData}
               labelMode={"floating"} onEditorEnterKey={onEnterKey} onInitialized={onInitialized}>
            {children}
        </Form>
    );
};

export default memo(CustomForm);
