// Write your Javascript code.
angular.module("journal", [])
    .controller("EditIssuesCtrl",[
        "$scope",
        function($scope) {

            $scope.issues = [
                {name: "test", description: "description"}
            ];
        }
    ]);