import Drawer from "devextreme-react/drawer";
import Content from "./Content";

const Body = ({content, reference}) => {
    return (
        <Drawer
            openedStateMode="shrink"
            defaultOpened={true}
            onInitialized={(e) => {
                reference.current = e.component;
            }}
            position="left"
            revealMode="expand"
            component={content}
            closeOnOutsideClick={false}
            height="calc(100vh - 56px)">
            <Content/>
        </Drawer>
    );
};

export default Body;