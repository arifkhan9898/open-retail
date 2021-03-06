﻿/**
 * Copyright (C) 2017 Kamarudin (http://coding4ever.net/)
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy of
 * the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 *
 * The latest version of this file can be found at https://github.com/rudi-krsoftware/open-retail
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using log4net;
using OpenRetail.Model;
using OpenRetail.Model.Report;
using OpenRetail.Bll.Api;
using OpenRetail.Bll.Service;
using OpenRetail.Helper;
using OpenRetail.Report;
using OpenRetail.Bll.Api.Report;
using OpenRetail.Bll.Service.Report;
using ConceptCave.WaitCursor;
using Microsoft.Reporting.WinForms;
using OpenRetail.Helper.UI.Template;

namespace OpenRetail.App.Laporan
{
    public partial class FrmLapPenggajianKaryawan : FrmSettingReportStandard
    {
        private IList<Karyawan> _listOfKaryawan = new List<Karyawan>();
        private ILog _log;
        
        public FrmLapPenggajianKaryawan(string header)
        {
            InitializeComponent();
            base.SetHeader(header);
            base.SetCheckBoxTitle("Pilih Karyawan");
            base.SetToolTip("Cari Karyawan ...");
            base.ReSize(120);

            _log = MainProgram.log;

            chkTampilkanNota.Visible = false;

            LoadKaryawan();
            LoadBulanDanTahun();            
        }

        private void LoadKaryawan()
        {
            IKaryawanBll bll = new KaryawanBll(_log);
            _listOfKaryawan = bll.GetAll();

            FillDataHelper.FillKaryawan(chkListBox, _listOfKaryawan);
        }

        private void LoadKaryawan(string name)
        {
            IKaryawanBll bll = new KaryawanBll(_log);
            _listOfKaryawan = bll.GetByName(name);

            FillDataHelper.FillKaryawan(chkListBox, _listOfKaryawan);
        }

        private void LoadBulanDanTahun()
        {
            FillDataHelper.FillBulan(cmbBulan, true);
            FillDataHelper.FillTahun(cmbTahun, true);
        }

        protected override void Cari()
        {
            LoadKaryawan(txtKeyword.Text);
        }

        protected override void Preview()
        {
            using (new StCursor(Cursors.WaitCursor, new TimeSpan(0, 0, 0, 0)))
            {
                PreviewReport();
            }
        }

        private IList<string> GetKaryawanId(IList<Karyawan> listOfKaryawan)
        {
            var listOfKaryawanId = new List<string>();

            for (var i = 0; i < chkListBox.Items.Count; i++)
            {
                if (chkListBox.GetItemChecked(i))
                {
                    var karyawan = listOfKaryawan[i];
                    listOfKaryawanId.Add(karyawan.karyawan_id);
                }
            }

            return listOfKaryawanId;
        }

        private void PreviewReport()
        {
            var periode = string.Empty;

            IReportGajiKaryawanBll reportBll = new ReportGajiKaryawanBll(_log);
            
            IList<ReportGajiKaryawan> listOfReportGajiKaryawan = new List<ReportGajiKaryawan>();
            IList<string> listOfKaryawanId = new List<string>();

            if (chkBoxTitle.Checked)
            {
                listOfKaryawanId = GetKaryawanId(_listOfKaryawan);

                if (listOfKaryawanId.Count == 0)
                {
                    MsgHelper.MsgWarning("Minimal 1 karyawan harus dipilih");
                    return;
                }
            }

            if (rdoTanggal.Checked)
            {
                if (!DateTimeHelper.IsValidRangeTanggal(dtpTanggalMulai.Value, dtpTanggalSelesai.Value))
                {
                    MsgHelper.MsgNotValidRangeTanggal();
                    return;
                }

                var tanggalMulai = DateTimeHelper.DateToString(dtpTanggalMulai.Value);
                var tanggalSelesai = DateTimeHelper.DateToString(dtpTanggalSelesai.Value);

                periode = dtpTanggalMulai.Value == dtpTanggalSelesai.Value ? string.Format("Periode : {0}", tanggalMulai) : string.Format("Periode : {0} s.d {1}", tanggalMulai, tanggalSelesai);

                listOfReportGajiKaryawan = reportBll.GetByTanggal(dtpTanggalMulai.Value, dtpTanggalSelesai.Value);
            }
            else
            {
                periode = string.Format("Periode : {0} {1}", cmbBulan.Text, cmbTahun.Text);

                var bulan = cmbBulan.SelectedIndex + 1;
                var tahun = int.Parse(cmbTahun.Text);

                listOfReportGajiKaryawan = reportBll.GetByBulan(bulan, tahun);
            }

            if (listOfKaryawanId.Count > 0 && listOfReportGajiKaryawan.Count > 0)
            {
                listOfReportGajiKaryawan = listOfReportGajiKaryawan.Where(f => listOfKaryawanId.Contains(f.karyawan_id))
                                                                                               .ToList();
            }
            
            if (listOfReportGajiKaryawan.Count > 0)
            {
                var reportDataSource = new ReportDataSource
                {
                    Name = "ReportGajiKaryawan",
                    Value = listOfReportGajiKaryawan
                };

                var parameters = new List<ReportParameter>();
                parameters.Add(new ReportParameter("periode", periode));

                base.ShowReport(this.Text, "RvGajiKaryawan", reportDataSource, parameters);
            }
            else
            {
                MsgHelper.MsgInfo("Maaf laporan data gaji karyawan tidak ditemukan");
            }
        }
    }
}
