import React from 'react';
import {SelectBox} from "devextreme-react/select-box";

const CustomSelectBox = (ds, name, display, references, onInitialized, onValueChanged) => ({data}) => {
    return (<SelectBox
        dataSource={ds}
        valueExpr="id"
        displayExpr={display}
        displayValue={display}
        defaultValue={data?.data[references] || data?.data[name]?.id}
        placeholder={"Selecciona una opción"}
        showDropDownButton={true}
        onValueChanged={e => {
            data.setValue(e.value)
            try {
                data.data[references] = e.value
                data.data[name].id = e.value
            } catch (error) {
            }
            if (onValueChanged) onValueChanged(e)
        }}
    />)
};

export default CustomSelectBox;
