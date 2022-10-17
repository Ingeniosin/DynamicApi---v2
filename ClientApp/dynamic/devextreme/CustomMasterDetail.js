/*
childs: () => <MasterDetail enabled={true} autoExpandAll={false} component={MasterDetail}/>
*/

/*
const MasterDetail = ({data: row}) => {
    const {data} = row;
    return (
        <CustomMasterDetail row={row}>
            
        </CustomMasterDetail>
    );
}; 
*/

const CustomMasterDetail = ({row, children}) => {
    return (
        <div className="row p-4" style={{backgroundColor: "white", padding: "10px", borderRadius: "10px", margin: "30px", boxShadow: "0 0 10px 0 rgba(0,0,0,0.1)"}}>
            {
                children
            }
        </div>
    )
};

export default CustomMasterDetail;