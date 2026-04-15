function Login(){

    function Login(e){
        const UserName ="filler";

        // hashed in caase message intercepted when sen to Api
        const Pasword = "filler";

        //code to send information to Api 

    }
    return(
    <>  
        <h2>AppSec</h2>

        <form>
            <p>UserName:</p>
            <input type="text"></input>

            <p>Password:</p>
            <input type="password"></input>

            <br/>

            <button onClick={Login}>Login</button>
        </form>
    </>
    );
}

export default Login