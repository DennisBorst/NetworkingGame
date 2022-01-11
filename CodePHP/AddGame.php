<?php 

include 'DatabaseConnection.php';

$playerOne = $_POST["playerOne"];
$_playerOne = $mysqli->real_escape_string($playerOne);

$playerTwo = $_POST["playerTwo"];
$_playerTwo = $mysqli->real_escape_string($playerTwo);

$winningPlayer = $_POST["winningPlayer"];
$_winningPlayer = (int)$mysqli->real_escape_string($winningPlayer);

$query = "INSERT INTO gamesPlayed (playerOne, playerTwo, winningPlayer) 
VALUES ('$_playerOne', '$_playerTwo', '$_winningPlayer')";


if (!($result = $mysqli->query($query))){
    echo "Error (" . $errornr . ") " . $error;
    http_response_code(500);
    die("Error (" . $errornr . ") " . $error);
}
    
$mysqli->close();
?>