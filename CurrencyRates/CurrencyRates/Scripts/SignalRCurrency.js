(function (angular) {
    function Controller($scope, $http) {
        var hubProxy = $.connection.currency;

        hubProxy.client.getAllData = function (data) {
            //console.log(JSON.stringify(data));
            $scope.currencyDatas = data;
            $scope.$apply();
        }
        hubProxy.client.UpdateData = function (data) {
            console.log("Update Data: " + JSON.stringify(data));
            for (var i = 0; i < $scope.currencyDatas.length; i++) {
                var index = isUpdated(data, $scope.currencyDatas[i]);
                if (index != -1) {
                    $scope.currencyDatas[i] = data[index];
                }
            };
            $scope.$apply();
        }

        //angular.forEach($scope.currencyDatas, function (currency) {
        //    var index = isUpdated(data, currency);
        //    if (index != -1) {
        //        currency = data[index];
        //    }
        //});
        //$scope.$apply();
        //}
        $.connection.hub.logging = true;
        $.connection.hub.start().done(function () {
            console.log("hub.start.done");
        }).fail(function (error) {
            console.log(error);
        });

        $.connection.hub.disconnected(function () {
            setTimeout(function () {
                $.connection.hub.start();
            }, 3000); // Restart connection after 1 seconds.
        });

    }
    var app = angular.module("app", ['pascalprecht.translate']).controller("Controller", ["$scope", "$http", Controller]);
    app.config(['$translateProvider', function ($translateProvider) {
        $translateProvider.translations('en', {
            'ABD DOLARI': 'USA DOLLAR',
            'AVUSTRALYA DOLARI': 'AUSTRALIAN DOLLAR',
            'BULGAR LEVASI': 'BULGARIAN LEV',
            'ÇİN YUANI': 'CHINESE RENMINBI',
            'DANİMARKA KRONU': 'DANISH KRONE',
            'İSVİÇRE FRANGI': 'SWISS FRANK',
            'İSVEÇ KRONU': 'SWEDISH KRONA',
            'İNGİLİZ STERLİNİ': 'GREAT BRITAIN POUND',
            'İRAN RİYALİ': 'IRANIAN RIAL',
            'JAPON YENİ': 'JAPENESE YEN',
            'KANADA DOLARI': 'CANADIAN DOLLAR',
            'KUVEYT DİNARI': 'KUWAITI DINAR',
            'NORVEÇ KRONU': 'NORWEGIAN KRONE',
            'PAKİSTAN RUPİSİ': 'PAKISTANI RUPEE',
            'RUMEN LEYİ': 'NEW LEU',
            'RUS RUBLESİ': 'RUSSIAN ROUBLE',
            'SUUDİ ARABİSTAN RİYALİ': 'SAUDI RIYAL',
        });

        $translateProvider.preferredLanguage('en');
        $translateProvider.useSanitizeValueStrategy('escape');
    }]);

})(angular);

function isUpdated(UpdatedList, currencyItem) {
    for (var i2 = 0; i2 < UpdatedList.length; i2++) {
        if (UpdatedList[i2].CurrencyName.indexOf(currencyItem.CurrencyName) >= 0) {
            return i2;
        }
    }
    return -1;
}