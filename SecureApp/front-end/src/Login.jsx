import { useState } from "react";
import { useNavigate } from "react-router-dom";

function Login(){
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const navigate = useNavigate();

    async function handleLogin(e){

        e.preventDefault();
        setError("");

        //console.log(username);
        //console.log(password);

        //code to send information to Api 
         try {
            const response = await fetch("https://localhost:7244/api/auth/login", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    username: username,
                    password: password
                })
            });

            //const data = await response.json();
            let data;

            if (response.ok) {
                data = await response.json();  // success = JSON
            } 
            else {
                const errorText = await response.text(); // error = plain text
                setError(errorText || "Login failed");
                console.log("Login failed:", errorText);
                return;
            }
            
            console.log("Login response:", data);

            //stop if login failed
            //if (!response.ok) {
            //    console.log("Login failed");
            //    return;
            //}

            //store token
            //localStorage.setItem("token", data.token);

            //const token = localStorage.getItem("token");

            // call protected route properly
            //const protectedResponse = await fetch("https://localhost:7244/api/auth/protected", {
            //    headers: {
            //        "Authorization": `Bearer ${token}`
            //    }
            //});

            //const protectedData = await protectedResponse.text();
            //console.log("Protected response:", protectedData);

            localStorage.setItem("token", data.token);
            localStorage.setItem("role", data.role);
            //redirect based on role
            if (data.role === "Admin") {
                navigate("/admin");
            } else {
                navigate("/user");
            }

        }
        catch (error) {
            console.error("Error connecting to API:", error);
            setError("Cannot connect to server");
        }
    }


    function handlePassword(e){
        setPassword(e.target.value);
    }
    function handleUsername(e){
        setUsername(e.target.value);
    }

    return(
    <>  
        <h2>AppSec - File Upload Portal</h2>

        {error && ( <p style={{ color: "red" }}>{error}</p> )}

        <form onSubmit={handleLogin}>
            <p>UserName:</p>
            <input type="text" onChange={handleUsername}/>

            <p>Password:</p>
            <input type="password" onChange={handlePassword}/>

            <br/>

            <button type="submit">Login</button>
        </form>
    </>
    );
}

export default Login