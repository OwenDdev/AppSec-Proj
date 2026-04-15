import { useState } from "react";

function Fileupload() {
    const [file, setFile] = useState(null);

    const token = localStorage.getItem("token");

    async function handleUpload() {
        if (!file) return;

        const formData = new FormData();
        formData.append("file", file);

        const response = await fetch("https://localhost:7244/api/file/upload", {
            method: "POST",
            headers: {
                "Authorization": `Bearer ${token}`
            },
            body: formData
        });

        const data = await response.json();
        console.log(data);
    }

    return (
        <>
            <h2>User view</h2>

            <input type="file" onChange={(e) => setFile(e.target.files[0])} />

            <button onClick={handleUpload}>
                Upload
            </button>
        </>
    );
}

export default Fileupload;