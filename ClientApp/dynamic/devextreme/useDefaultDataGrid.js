import DataGrid, {Column, Export, IDataGridOptions} from "devextreme-react/data-grid";
import {useEffect, useMemo, useRef, useState} from "react";
import {getDsOptions} from "../../api/api-service";
import set from "lodash.set";

class DataGridConfig {
    columns: Array<Column>;
    defaultValues: any;
    columnChooser: boolean = true;
    allowAdding: boolean = true;
    allowDeleting: boolean = true;
    allowUpdating: boolean = true;
    dataSource: { api: string; filter: Array<any>; pageSize: number, select: Array<any> } | Array<any>;
    editorMode: 'row' | 'cell'
    reference: any;
    otherConfigs: IDataGridOptions;
    childs: any;
    updateInterval: any;
}

const requiredRule = {type: 'required', message: 'El campo es requerido'};
const emailRule = {type: 'email', message: 'El campo debe ser un email válido'};


const useCustomGrid = (config: DataGridConfig) => {
    const divRef = useRef(null);
    const settings = useMemo(() => getGridProps(config), [config]);
    const columns = useMemo(() =>  settings.cols.map((col: Column) => <Column  {...col} key={col.dataField}/>), [settings.cols]);
    const childs = useMemo(() =>  settings.childs(), [settings.childs]);

    useEffect(() => {
        const updateInterval = settings.updateInterval;
        if (!updateInterval) return;
        let interval = setInterval(async () => {await settings.dataSource.reload();}, updateInterval);
        const observer = new IntersectionObserver(
            ([entry]) => {
                const visible = entry.isIntersecting;
                if(!visible) {
                    clearInterval(interval);
                    interval = null;
                    return;
                }
                if(interval) return;
                interval = setInterval(async () => {await settings.dataSource.reload();}, updateInterval);
            },
        )
        observer.observe(divRef.current)
        
        return () => clearInterval(interval)
    }, []);
    
    return (
        <div ref={divRef}>
            <DataGrid {...settings}>
                {columns}
                {childs}
                <Export enabled={true}/>
            </DataGrid>
        </div>
    
    );
}

const getGridProps = (config: DataGridConfig): IDataGridOptions => {
    let {defaultValues, childs, updateInterval, otherConfigs = {}, reference = {}, columnChooser = false, allowAdding = true, allowDeleting = true, allowUpdating = true, dataSource = [], columns, editorMode = 'cell'} = config;
    const isApi = typeof dataSource === 'object' && dataSource.api;
    if (isApi) {
        dataSource = getDsOptions(dataSource.api, {
            filter: dataSource.filter,
            select: [...columns.filter(c => c.dataField).map(c => c.dataField), ...(dataSource.select || [])],
            paginate: true,
            pageSize: dataSource.pageSize,
        })
        
    }

    columns = columns.map(column => {
        if(column.format) {
            set(column, 'editorOptions.format', column.format);
        }

        const validationRules = column.validationRules || [];

        if (column.required && column.dataType === 'boolean') {
            column.required = false;
        }

        if (column.required) {
            column.validationRules = [...validationRules, requiredRule];
        }

        if (column.email) {
            column.validationRules = [...validationRules, emailRule];
        }

        if (column.phone) {
            column.dataType = 'number';
            column.setCellValue = (rowData, value) => {
                rowData[column.dataField] = value.toString();
            }
        }

        return column;
    })
    return {
        onInitialized: ({component}) => {
            reference.current = {
                instance: component,
                refresh: async () => await component.refresh(),
                clearFilter: () => component.clearFilter(),
                clearSelection: () => component.clearSelection(),
                data: () => component.getDataSource().items(),
            };            
        },
        updateInterval,
        showColumnHeaders: true,
        columnHidingEnabled: false,
        childs: childs || (() => null),
        cols: columns,
        loadPanel: {
            enabled: !updateInterval,
            animation: true,
            shading: true,
        },
        remoteOperations: {
            filtering: true,
            paging: true,
            sorting: true,
        },
       /* allowColumnResizing: true,*/
        highlightChanges: false,
        activeStateEnabled: false,
        cellHintEnabled: false,
        autoNavigateToFocusedRow: false,
        hoverStateEnabled: false,
        defaultSearchPanel: {
            visible: false,
            placeholder: 'Buscar'
        },
        dataSource: dataSource,
        defaultEditing: {
            mode: editorMode,
            allowAdding: allowAdding,
            allowDeleting: allowDeleting,
            allowUpdating: allowUpdating,
            useIcons: true,
            confirmDelete: false,
            editColumnName: true,
            texts: {
                addRow: "Añadir",
                cancelAllChanges: "Cancelar",
                cancelRowChanges: "Cancelar",
                confirmDeleteMessage: "¿Está seguro de que desea eliminar este registro?",
                deleteRow: "Eliminar",
                editRow: "Editar",
                saveAllChanges: "Guardar",
                saveRowChanges: "Guardar",
                undeleteRow: "Deshacer eliminación",
                validationCancelChanges: "Cancelar",
            }
        },
        repaintChangesOnly: true,
        showBorders: true,
        columnAutoWidth: true,
        wordWrapEnabled: false,
        allowColumnReordering: true,
        showColumnLines: true,
        columnResizingMode: 'widget',
        columnChooser: {
            enabled: columnChooser,
            allowSearch: true,
            mode: "select",
            title: "Selecciona las columnas",
            emptyPanelText: "No hay columnas que mostrar",
        },
        noDataText: '¡Nada por aqui!',
        onInitNewRow: e => {
            e.data = defaultValues;
        },
        ...otherConfigs
    }
}

export default useCustomGrid;
