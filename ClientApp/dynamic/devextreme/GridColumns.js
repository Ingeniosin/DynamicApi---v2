import React from 'react';
import {Column, EmailRule, Lookup, NumericRule, RequiredRule, StringLengthRule} from "devextreme-react/data-grid";

export const ColumnTypes = {
    String: 'string',
    Number: 'number',
    Boolean: 'boolean',
    Date: 'date',
    Object: 'object',
    DateTime: 'datetime',
}

export const GridColumns = ({columns = []}) => {
/*
    columns?.push({dataField: 'id', type: ColumnTypes.Number, name: 'id', sortOrder: 'asc', visible: false, allowEditing: false})
*/
    return columns?.map(({name, editorOptions, dataField, type, isRequired, isEmail, lookup, sortOrder, isMoney, format, isPhone, allowEditing = true, visible}) => {
        if (lookup) {
            type = ColumnTypes.String
        }
        const displayName = name ?? dataField;
        return <Column sortOrder={sortOrder} format={isMoney ? 'currency' : format} allowEditing={allowEditing} dataType={type} visible={visible} dataField={dataField} key={dataField} caption={displayName} editorOptions={{
            ...editorOptions,
            format: isMoney ? 'currency' : format,
        }}>
            {
                lookup && <Lookup dataSource={lookup.dataSource} valueExpr={lookup.valueExpr} displayExpr={lookup.displayExpr}/>
            }
            {
                isRequired && <RequiredRule message={"¡El campo '"+displayName+"' es requerido!'"} />
            }
            {
                isPhone && (<StringLengthRule min={10} max={10} message={"El campo '"+displayName+"' es un telefono celular invalido."} />)
            }
            {
                isEmail && <EmailRule message={"El campo '"+displayName+"' es una direction de correo invalida."} />
            }
        </Column>
    })
};

export default GridColumns;
