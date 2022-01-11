<?php 

include 'DatabaseConnection.php';

$id = $_POST["id"];
$_id = $mysqli->real_escape_string($id);

$totalGamesQuery = "SELECT *
FROM gamesPlayed 
WHERE playerOne='$_id' OR playerTwo='$_id'";

$resultTotalGames = $mysqli->query($totalGamesQuery);

$totalWinsQuery = "SELECT *
FROM gamesPlayed 
WHERE winningPlayer='$_id'";

$resultTotalWins = $mysqli->query($totalWinsQuery);

echo $resultTotalGames->num_rows . "_" . $resultTotalWins->num_rows;

// if ($result && mysqli_num_rows($result) <= 0){
//     http_response_code(500);
//     die("Error (" . $errornr . ") " . $error);
// }

$mysqli->close();
?>