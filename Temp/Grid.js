import {useMemo} from "react";

const captions = {
    name: 'Name', 
	startDate: 'Start Date', 
	endDate: 'End Date'
}

const columns = [
    {dataField: 'name', dataType: 'string', caption: captions['name'], required: true}, 
	{dataField: 'startDate', dataType: 'datetime', caption: captions['startDate'], required: true}, 
	{dataField: 'endDate', dataType: 'datetime', caption: captions['endDate'], required: true}
]

const Grid = ({filter, reference}) => {
    const configuration = useMemo(() => ({
        reference,
        columns,
        editorMode: 'cell',
        dataSource: {
            api: "PayrollBooks",
            pageSize: 10,
            filter
        }
    }), [filter, reference]);
    return useCustomGrid(configuration)
};

export default Grid;