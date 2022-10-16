import DataGrid from "devextreme-react/data-grid";
import {memo, useRef} from "react";

const CustomDataGrid = ({options}) => {

    const dataSource = useRef(options.dataSource)

    return <DataGrid {...options} dataSource={dataSource.current}/>;
};

export default memo(CustomDataGrid);
