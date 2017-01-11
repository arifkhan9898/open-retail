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

using OpenRetail.Model;
using OpenRetail.Bll.Api;
using OpenRetail.Bll.Service;
using OpenRetail.App.UI.Template;
using OpenRetail.App.Helper;
using Syncfusion.Windows.Forms.Grid;
using ConceptCave.WaitCursor;
using OpenRetail.App.UserControl;

namespace OpenRetail.App.Referensi
{
    public partial class FrmListProduk : FrmListEmptyBody, IListener
    {
        private IProdukBll _bll; // deklarasi objek business logic layer 
        private IList<Produk> _listOfProduk = new List<Produk>();
        private IList<Golongan> _listOfGolongan = new List<Golongan>();

        public FrmListProduk(string header)
            : base()
        {
            InitializeComponent();

            base.SetHeader(header);
            base.WindowState = FormWindowState.Maximized;

            _bll = new ProdukBll();

            LoadDataGolongan();

            InitGridList();
        }

        private void LoadDataGolongan()
        {
            IGolonganBll golonganBll = new GolonganBll();

            using (new StCursor(Cursors.WaitCursor, new TimeSpan(0, 0, 0, 0)))
            {
                _listOfGolongan = golonganBll.GetAll();

                cmbGolongan.Items.Clear();
                cmbGolongan.Items.Add("-- Semua --");
                foreach (var golongan in _listOfGolongan)
                {
                    cmbGolongan.Items.Add(golongan.nama_golongan);
                }

                cmbGolongan.SelectedIndex = 0;
            }
        }

        private void LoadDataProduk(string golonganId = "")
        {
            using (new StCursor(Cursors.WaitCursor, new TimeSpan(0, 0, 0, 0)))
            {
                if (golonganId.Length > 0)
                    _listOfProduk = _bll.GetByGolongan(golonganId);
                else
                    _listOfProduk = _bll.GetAll();

                GridListControlHelper.Refresh<Produk>(this.gridList, _listOfProduk);
            }

            ResetButton();
        }

        private void LoadDataProdukByName(string name)
        {
            using (new StCursor(Cursors.WaitCursor, new TimeSpan(0, 0, 0, 0)))
            {
                _listOfProduk = _bll.GetByName(name);
                GridListControlHelper.Refresh<Produk>(this.gridList, _listOfProduk);
            }

            ResetButton();
        }

        private void ResetButton()
        {
            base.SetActiveBtnPerbaikiAndHapus(_listOfProduk.Count > 0);
        }

        private void InitGridList()
        {
            var gridListProperties = new List<GridListControlProperties>();

            gridListProperties.Add(new GridListControlProperties { Header = "No", Width = 30 });
            gridListProperties.Add(new GridListControlProperties { Header = "Kode Produk", Width = 130 });
            gridListProperties.Add(new GridListControlProperties { Header = "Nama Produk", Width = 540 });
            gridListProperties.Add(new GridListControlProperties { Header = "Satuan", Width = 130 });
            gridListProperties.Add(new GridListControlProperties { Header = "Harga Beli", Width = 110 });
            gridListProperties.Add(new GridListControlProperties { Header = "Harga Jual", Width = 110 });
            gridListProperties.Add(new GridListControlProperties { Header = "Stok", Width = 90 });
            gridListProperties.Add(new GridListControlProperties { Header = "Stok Gudang", Width = 90 });
            gridListProperties.Add(new GridListControlProperties { Header = "Min. Stok Gudang", Width = 110 });

            GridListControlHelper.InitializeGridListControl<Produk>(this.gridList, _listOfProduk, gridListProperties);

            if (_listOfProduk.Count > 0)
                this.gridList.SetSelected(0, true);

            this.gridList.Grid.QueryCellInfo += delegate(object sender, GridQueryCellInfoEventArgs e)
            {

                if (_listOfProduk.Count > 0)
                {
                    if (e.RowIndex > 0)
                    {

                        var rowIndex = e.RowIndex - 1;

                        if (rowIndex < _listOfProduk.Count)
                        {
                            var obj = _listOfProduk[rowIndex];

                            switch (e.ColIndex)
                            {
                                case 2:
                                    e.Style.CellValue = obj.kode_produk;
                                    break;

                                case 3:
                                    e.Style.CellValue = obj.nama_produk;
                                    break;

                                case 4:
                                    var satuan = string.Empty;

                                    if (obj.satuan.Length > 0)
                                        satuan = obj.satuan;

                                    e.Style.CellValue = satuan;
                                    break;

                                case 5:
                                    e.Style.CellValue = NumberHelper.NumberToString(obj.harga_beli);
                                    e.Style.HorizontalAlignment = GridHorizontalAlignment.Right;
                                    break;

                                case 6:
                                    e.Style.CellValue = NumberHelper.NumberToString(obj.harga_jual);
                                    e.Style.HorizontalAlignment = GridHorizontalAlignment.Right;
                                    break;

                                case 7:
                                    e.Style.CellValue = obj.stok;
                                    e.Style.HorizontalAlignment = GridHorizontalAlignment.Center;
                                    break;

                                case 8:
                                    e.Style.CellValue = obj.stok_gudang;
                                    e.Style.HorizontalAlignment = GridHorizontalAlignment.Center;
                                    break;

                                case 9:
                                    e.Style.CellValue = obj.minimal_stok_gudang;
                                    e.Style.HorizontalAlignment = GridHorizontalAlignment.Center;
                                    break;

                                default:
                                    break;
                            }

                            // we handled it, let the grid know
                            e.Handled = true;
                        }
                    }
                }
            };
        }        

        private void cmbGolongan_SelectedIndexChanged(object sender, EventArgs e)
        {
            var golonganId = string.Empty;

            var index = ((ComboBox)sender).SelectedIndex;

            if (index > 0)
            {
                var golongan = _listOfGolongan[index - 1];
                golonganId = golongan.golongan_id;
            }

            LoadDataProduk(golonganId);
        }

        protected override void Tambah()
        {
            if (cmbGolongan.SelectedIndex == 0)
            {
                var msg = "Maaf data 'Golongan' belum dipilih.";
                MsgHelper.MsgWarning(msg);

                return;
            }

            var golongan = _listOfGolongan[cmbGolongan.SelectedIndex - 1];

            var frm = new FrmEntryProduk("Tambah Data " + this.Text, golongan, _listOfGolongan, _bll);
            frm.Listener = this;
            frm.ShowDialog();
        }

        protected override void Perbaiki()
        {
            var index = this.gridList.SelectedIndex;

            if (!base.IsSelectedItem(index, this.TabText))
                return;

            var produk = _listOfProduk[index];
            produk.kode_produk_old = produk.kode_produk;

            var frm = new FrmEntryProduk("Edit Data " + this.Text, produk, _listOfGolongan, _bll);
            frm.Listener = this;
            frm.ShowDialog();
        }

        protected override void Hapus()
        {
            var index = this.gridList.SelectedIndex;

            if (!base.IsSelectedItem(index, this.TabText))
                return;

            if (MsgHelper.MsgDelete())
            {
                var produk = _listOfProduk[index];

                var result = _bll.Delete(produk);
                if (result > 0)
                {
                    GridListControlHelper.RemoveObject<Produk>(this.gridList, _listOfProduk, produk);
                    ResetButton();
                }
                else
                    MsgHelper.MsgDeleteError();
            }
        }

        public void Ok(object sender, object data)
        {
            throw new NotImplementedException();
        }

        public void Ok(object sender, bool isNewData, object data)
        {
            var produk = (Produk)data;

            if (isNewData)
            {
                GridListControlHelper.AddObject<Produk>(this.gridList, _listOfProduk, produk);
                ResetButton();
            }
            else
                GridListControlHelper.UpdateObject<Produk>(this.gridList, _listOfProduk, produk);
        }

        private void gridList_DoubleClick(object sender, EventArgs e)
        {
            if (btnPerbaiki.Enabled)
                Perbaiki();
        }

        private void txtNamaProduk_Enter(object sender, EventArgs e)
        {
            var text = (AdvancedTextbox)sender;

            text.Clear();
            text.ForeColor = Color.Black;
        }

        private void txtNamaProduk_Leave(object sender, EventArgs e)
        {
            var text = (AdvancedTextbox)sender;

            if (string.IsNullOrEmpty(text.Text))
            {
                text.Text = "Ketik nama produk ...";
                text.ForeColor = Color.Gray;
            }        
        }

        private void btnCari_Click(object sender, EventArgs e)
        {
            LoadDataProdukByName(txtNamaProduk.Text);
        }

        private void txtNamaProduk_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (KeyPressHelper.IsEnter(e))
                btnCari_Click(sender, e);
        }
    }
}
