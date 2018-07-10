
// For Default Alert


var htmlMessageContent = '<div class="swal2-modal" style="display: none; width: 500px; padding: 20px; background: rgb(255, 255, 255); min-height: 167px;" tabindex="-1">';
htmlMessageContent += '  <h2>Title</h2><div class="swal2-content" style="display: block;">messageconent</div>';
htmlMessageContent += '  <hr class="swal2-spacer" style="display: block;">';
htmlMessageContent += '  <button type="button" class="swal2-confirm swal2-styled" style="background-color: rgb(48, 133, 214); border-left-color: rgb(48, 133, 214); border-right-color: rgb(48, 133, 214);" onclick="CloseAlert(this);">OK</button>';
htmlMessageContent += '  <span class="swal2-close" style="display: block; font-size:20px" onclick="CloseAlert(this);">x</span>';
htmlMessageContent += '  </div>';


window.alert = function (message, fallback) {
  CreateAlert(message, fallback);
};

function CreateAlert(message) {

  var divdialog = document.createElement('div');
  var id = 'SkyExAlert_' + Math.floor((Math.random() * 100000000) + 1);;
  $(divdialog).attr('id', id);
  $(divdialog).attr('class', 'swal2-container swal2-fade SkyExAlertPopup');
  var temp = htmlMessageContent.replace("messageconent", message);
  $(divdialog).html(temp);
  $(divdialog).css({ 'overflow-y': 'auto' });
  $('body').append(divdialog);
  ShowNextAlert();
}

function CloseAlert(btn) {
  var _parent = $(btn).parents('div.SkyExAlertPopup');
  var id = $(_parent).attr('id');
  //console.log(id);
  $("#" + id).remove();
  ShowNextAlert();
};

function ShowNextAlert() {
  $('.swal2-modal').addClass('swal2-fade');
  var modalFisrt = $('.SkyExAlertPopup')[0];
  //debugger;
  var id = $(modalFisrt).attr("id");
  $(modalFisrt).find('button').focus();
  //console.log(id);
  $("#" + id).addClass('swal2-in');
  $("#" + id + ' div.swal2-modal').removeClass('swal2-fade');
  $("#" + id + ' div.swal2-modal').addClass('swal2-show');
  $("#" + id + ' div.swal2-modal').show();

}

$(function () {
  $(document).on("keydown", function (e) {
    var btn = $('.SkyExAlertPopup.swal2-in').find('button');
    if (btn !== undefined && e.which == 13 || e.which == 27) {
      //console.log(btn);
      $(btn).trigger('click');
    }
  });
});
var IsClickClosed = false;
//  For 3 custom buttons
$(document).on('click', '.swalbtn-save', function () {
  swal.clickConfirm();
});
$(document).on('click', '.swalbtn-discard', function () {
  swal.clickCancel();
});
$(document).on('click', '.swalbtn-cancel', function () {
  $(".swal2-close").click();
});

var SkyEx = (function () {
  return {
    Alert: function (message, fallback) {
      CreateAlert(message, fallback);
    },
    Prompt: function (message) {
      return swal({ title: 'Title', text: message, confirmButtonText: 'OK', showCloseButton: true })
        .then(function (ok) {
          return ok;
        }, function (dismiss) {
          if (dismiss === 'cancel' || dismiss === 'close') { // you might also handle 'close' or 'timer' if you used those
            // ignore
            return false;
          } else {
            throw dismiss;
          }
        });
    },
    Confirm: function (confirmMessage, type) {
      // for custom 3 buttons
      if (type !== undefined && type == 3) {
        return swal({
          title: "Title",
          html: confirmMessage +
          "<br>" +
          '<button type="button" role="button" tabindex="0" class="swalbtn swalbtn-save"> Save </button>' +
          '<button type="button" role="button" tabindex="1" class="swalbtn swalbtn-discard"> Discard </button>' +
          '<button type="button" role="button" tabindex="2" class="swalbtn swalbtn-cancel"> Cancel </button>',
          showCancelButton: false,
          showCloseButton:false,
          showConfirmButton: false,
          //onClose: DeleteUnsaved
        }).then(function (ok) {
          return "save";
        }, function (dismiss) {
          // dismiss can be "cancel" | "close" | "outside"
          if (dismiss == "cancel") {
            return "discard";
          }
          else {
            return dismiss;
          }
        });
      }
      else if (type !== undefined && type == 2) {
        return swal({
          title: "Title",
          text: confirmMessage,
          showCancelButton: true,
          confirmButtonText: "Yes",
          cancelButtonText: "No",
          cancelButtonClass: "btn-danger"
        }).then(function (ok) {
          return true;
        }, function (dismiss) {
          // dismiss can be "cancel" | "close" | "outside"
          return false;
        });
      }
      else if (type !== undefined && type == 1) {
        return swal({
          title: "Title",
          text: confirmMessage,
          showCancelButton: true,
          confirmButtonText: "Ok",
          cancelButtonText: "Cancel",
          cancelButtonClass: "btn-danger"
        }).then(function (ok) {
          return true;
        }, function (dismiss) {
          // dismiss can be "cancel" | "close" | "outside"
          return false;
        });
      }
      else {
        return swal({
          title: "Title",
          text: confirmMessage,
          showCancelButton: true,
          confirmButtonText: "Ok",
          cancelButtonText: "Cancel",
          cancelButtonClass: "btn-danger"
        }).then(function (ok) {
          return true;
        }, function (dismiss) {
          // dismiss can be "cancel" | "close" | "outside"
          return false;
        });
      }
    }

  }

})(SkyEx || {})


// For Report Dailog 

var htmlReportdialog = '<div class="modal-dialog">';
htmlReportdialog += '   <div class="modal-content" style="width: 750px; top: 10%; background-color: #fff; min-height:400px;">';
htmlReportdialog += '     <fieldset>';
htmlReportdialog += '       <legend class="col-md-12 col-xs-12  pull-Left text-Left LegendEvent">';
htmlReportdialog += '         <span class="oe-diagnosis"></span>';
htmlReportdialog += '         <span class="LegendTitle">Title</span>';
htmlReportdialog += '         <button class="close" data-dismiss="modal" type="button" onclick="SkyExUIReport.CloseDialog(this);">';
htmlReportdialog += '           <span class="oe-cancel"></span>';
htmlReportdialog += '         </button>    ';
htmlReportdialog += '       </legend>';
htmlReportdialog += '       <div class="row">';
htmlReportdialog += '         <div id="reportcontainer" class="reportcontainer" style="height: 900px !important; margin-left:5px; padding:0 30px">';
htmlReportdialog += '			<iframe id="idIframeReport" height="95%" width="95%" src="" />';
htmlReportdialog += '         </div>';
htmlReportdialog += '       </div>';
htmlReportdialog += '     </fieldset>';
htmlReportdialog += '   </div>';
htmlReportdialog += ' </div>';





var SkyExUIReport = (function () {
  return {
    Dialog: function (apiUrl, reportName, params) {
      _url = apiUrl + reportName;
      $.getJSON(_url, function (res) {
        if (res) {
          _reportUrl = res.Data[0].ReportServer + reportName + '&';

          var divreport = document.createElement('div');
          $(divreport).attr('id', 'SystemReportModal');
          $(divreport).attr('class', 'modal fade SystemReportModal in');
          $(divreport).attr('role', 'dialog');
          $(divreport).html(htmlReportdialog);
          $('body').append(divreport);
          if (params !== undefined) {
            params["rc:Parameters"] = false;
            _reportUrl = _reportUrl + jQuery.param(params);
          }
          var _iframe = document.getElementById('idIframeReport');
          $(_iframe).attr('src', _reportUrl);
          $(divreport).show();
        }
      });
    },
    CloseDialog: function (btn) {
      var _parent = $(btn).parents('div.SystemReportModal');
      $(_parent).remove();
    }
  }

})(SkyExUIReport || {})
