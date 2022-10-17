import TreeView from "devextreme-react/tree-view";
import {useNavigate} from "react-router-dom";
import {routes} from "../../Configuration";


const DrawerContent = () => {
    const navigate = useNavigate();
    const currentPath = window.location.pathname;
    
    const routesItem = routes.map((route) => {
        return {
            ...route,
            selected: route.route === currentPath.toLowerCase(),
        };
    });
    
    return (
        <div className="mt-3" style={{width: "240px", paddingRight: 15, height: "100vh", backgroundColor: "transparent",}}>
            <TreeView
                items={routesItem}
                onItemClick={(e) => {
                    const {route, selected} = e.itemData;
                    if(selected) return;
                    navigate(route);
                }}
                width={"100%"}
            />
        </div>
    );
};

export default DrawerContent;