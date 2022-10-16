import {Route, Routes} from "react-router-dom";
import Asesores from "../pages/Asesores";
import Configuracion from "../pages/Configuracion";
import Clientes from "../pages/Clientes";

const Content = () => {
    return (
        <div style={{
            height: "calc(100vh - 56px)", 
            backgroundColor: "#f9f9f9",
            borderLeft: "2px solid #e5e5e5",
            borderTop: "2px solid #e5e5e5",
            overflowY: "scroll"
        }}>
            <div className="container py-5">
                <div style={{
                    backgroundColor: "#fff",
                    borderRadius: "10px",
                    padding: "20px",
                    boxShadow: "0 0 10px rgba(0,0,0,0.1)"
                }}>
                    <Routes>
                        <Route path="/" element={<Asesores/>}/>
                        <Route path="/clients" element={<Clientes/>}/>
                        <Route path="/config" element={<Configuracion/>}/>
                    </Routes>
                </div>               
            </div>
        </div>
    );
};

export default Content;