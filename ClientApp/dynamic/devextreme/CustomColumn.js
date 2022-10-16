import CustomSelectBox from "./CustomSelectBox";

const CustomColumn = (ds, name, convertName = name+"Id") => {
    return {
        dataField:convertName,
        editCellComponent: CustomSelectBox(ds, name),
        lookup: {dataSource: ds, displayExpr: "name", valueExpr: "id"},
        calculateDisplayValue:data => data[name]?.name
    };
};

export default CustomColumn;
