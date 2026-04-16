import Login from "./Login.jsx"
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Fileupload from "./Fileupload.jsx";
import Admin from "./Admin.jsx";
import Signup from "./Signup.jsx";


function App() {
  return (
    <>
      <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/signup" element={<Signup />} />
        <Route path="/user" element={<Fileupload />} />
        <Route path="/admin" element={<Admin />} />
      </Routes>
    </>
  )
}

export default App
