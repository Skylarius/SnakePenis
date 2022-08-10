<?php
 
require ('../snakepenis/mysqli-connect-snakepenis.php');
  
// Strings must be escaped to prevent SQL injection attack.
$id = mysqli_real_escape_string($dbc, $_GET['id']);
$name = mysqli_real_escape_string($dbc, $_GET['name']);
$score = mysqli_real_escape_string($dbc, $_GET['score']);
$length = mysqli_real_escape_string($dbc, $_GET['length']);
$hash = $_GET['hash'];
 
$secretKey="Laola"; # Change this value to match the value stored in the client script
 
$real_hash = md5($name . $score . $secretKey);
if($real_hash == $hash) {
    // Send variables for the MySQL database class.
    echo "Hash match succesful!";
    $query_insert = "INSERT INTO SnakePenisScore VALUES  ('$id', '$name', '$length', '$score');";
    $query_update = "UPDATE SnakePenisScore SET SCORE='$score', LENGTH='$length' WHERE ID='$id' AND NAME='$name' AND SCORE < '$score';";
    $result = $dbc->query($query_insert) or $dbc->query($query_update) or die('Query failed: ' . mysqli_error($dbc));
} else {
    echo $hash . " is not the real hash." . "\n";
}

?>