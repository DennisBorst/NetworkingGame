<?php 

include 'DatabaseConnection.php';

$query = "SELECT COUNT(id) AS total_games
FROM gamesPlayed 
WHERE date BETWEEN NOW() - INTERVAL 1 MONTH - INTERVAL 1 DAY AND NOW()";

$result = $mysqli->query($query);

if ($result->num_rows > 0){
    while($row = $result->fetch_assoc()) {
        echo $row["total_games"];
    }
} else {
    http_response_code(500);
}

$mysqli->close();
?>