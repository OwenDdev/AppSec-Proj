import { useState } from "react";

function Login(){
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");

    async function handleLogin(e){

        e.preventDefault();
        console.log(username);
        console.log(password);

         //code to send information to Api 
        try {
            const response = await fetch("http://localhost:5000/api/login", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    username: username,
                    password: password
                })
            });

            // store response form Api
            const data = await response.json();
            console.log(data);
        }
        catch (error) {
        console.error("Error connecting to API:", error);
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
        <h2>AppSec</h2>

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