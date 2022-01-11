<?php
include 'DatabaseConnection.php';

$name = $_POST["username"];
$_name = $mysqli->real_escape_string($name);

$password = $_POST["password"];
$_password = $mysqli->real_escape_string($password);

$query = "SELECT *
FROM users 
WHERE username='$_name' AND password='$_password'";

$result = $mysqli->query($query);

if ($result->num_rows > 0){
    while($row = $result->fetch_assoc()) {
        echo $row["id"];
    }
} else {
    http_response_code(500);
}

if ($result && mysqli_num_rows($result) <= 0){
    http_response_code(500);
    die("Error (" . $errornr . ") " . $error);
}

$mysqli->close();

?>