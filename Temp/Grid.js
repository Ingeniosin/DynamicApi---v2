import {useMemo} from "react";

const captions = {
    ip: 'Ip', 
	userAgent: 'User Agent', 
	date: 'Date'
}

const columns = [
    {dataField: 'ip', dataType: 'string', caption: captions['ip'], required: true}, 
	{dataField: 'userAgent', dataType: 'string', caption: captions['userAgent'], required: true}, 
	{dataField: 'date', dataType: 'datetime', caption: captions['date'], required: true}
]

const Grid = ({reference, versionId}) => {
    const configuration = useMemo(() => ({
        reference,
        columns,
        defaultValues: {
            versionId
        },
        editorMode: 'cell',
        dataSource: {
            api: "CheckRequests",
            pageSize: 10,
            filter: ['VersionId', '=', versionId]
        }
    }), [reference]);
    return useCustomGrid(configuration)
};

export default Grid;