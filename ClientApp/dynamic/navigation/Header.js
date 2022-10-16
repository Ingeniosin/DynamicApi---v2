﻿import Button from "devextreme-react/button";
import logo from '../../assets/icons8_messages_50px.png';

const Header = ({drawerRef}) => {
    const onClickBtnMenu = () => {
        drawerRef.current.toggle();
    };
    return (
        <div className="d-flex align-items-center" style={{width: "100vw", padding: "0px 15px", height: "56px",  fontSize: "60px",}}>
            <Button icon="menu" stylingMode="text" onClick={onClickBtnMenu} style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                width: "34px",
                height: "34px",
            }} />

            <div style={{
                alignItems: "center",
                display: "flex",
                paddingLeft: "10px",
            }}>
                <img src={logo} alt="logo" style={{width: "30px", height: "30px"}} />
                <h6 className="mt-2" >MyAutoBot</h6>
            </div>
       
            
        </div>
    )
};

export default Header;