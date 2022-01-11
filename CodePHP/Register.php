<?php 
include 'DatabaseConnection.php'; 

$name = $_POST["username"];
$_name = $mysqli->real_escape_string($name);

$password = $_POST["password"];
$_password = $mysqli->real_escape_string($password);

$query = "INSERT INTO users (username, password) VALUES ('$_name', '$_password')";

if (!($result = $mysqli->query($query))){
    http_response_code(500);
    die("Error (" . $errornr . ") " . $error);
}
    
$mysqli->close();
?>