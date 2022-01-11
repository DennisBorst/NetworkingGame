<?php 

include 'DatabaseConnection.php';

$query = "SELECT users.username AS winning_player, COUNT(winningPlayer) AS magnitude
FROM gamesPlayed
JOIN users ON users.id = winningPlayer
WHERE date BETWEEN NOW() - INTERVAL 1 MONTH - INTERVAL 1 DAY AND NOW()
GROUP BY users.username
ORDER BY magnitude ASC
LIMIT 5";

$result = $mysqli->query($query);

if ($result->num_rows > 0){
    while($row = $result->fetch_assoc()) {
        echo $row["winning_player"] . "_" . $row["magnitude"] . "-";
    }
} else {
    http_response_code(500);
}

$mysqli->close();
?>