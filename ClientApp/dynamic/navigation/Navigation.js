import {Toaster} from "react-hot-toast";
import {useRef} from "react";
import Header from "./Header";
import Body from "./Body";
import DrawerContent from "./DrawerContent";

const App = () => {
    const drawerRef = useRef(null);
    return (
        <>
            <Header drawerRef={drawerRef}/>
            <Body reference={drawerRef} content={DrawerContent}/>
            <Toaster position="bottom-right"/>
        </>
    );
};

export default App;
