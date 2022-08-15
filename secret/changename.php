<?php
 
require ('../snakepenis/mysqli-connect-snakepenis.php');
  
// Strings must be escaped to prevent SQL injection attack.
$id = mysqli_real_escape_string($dbc, $_GET['id']);
$name = mysqli_real_escape_string($dbc, $_GET['name']);

$hash = $_GET['hash'];
 
$secretKey="Laola"; # Change this value to match the value stored in the client script
 
$real_hash = md5($name . $id . $secretKey);
if($real_hash == $hash) {
    // Send variables for the MySQL database class.
    echo "Hash match succesful!";
    $query_update = "UPDATE SnakePenisScore SET NAME='$name' WHERE ID='$id';";
    $result = $dbc->query($query_update) or die('Query failed: ' . mysqli_error($dbc));
} else {
    echo $hash . " is not the real hash." . "\n";
}

?>